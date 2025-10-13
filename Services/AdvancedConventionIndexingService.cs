using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;
using LocalRAG.Data;
using LocalRAG.Services.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalRAG.Services
{
    public class AdvancedConventionIndexingService
    {
        private readonly IRagService _ragService;
        private readonly ILogger<AdvancedConventionIndexingService> _logger;
        private readonly ConventionDbContext _context;
        private readonly ConventionDocumentBuilder _documentBuilder;

        public AdvancedConventionIndexingService(
            IRagService ragService,
            ILogger<AdvancedConventionIndexingService> logger,
            ConventionDbContext context,
            ConventionDocumentBuilder documentBuilder)
        {
            _ragService = ragService;
            _logger = logger;
            _context = context;
            _documentBuilder = documentBuilder;
        }

        public async Task<IndexingResult> ReindexAllConventionsAsync()
        {
            var result = new IndexingResult();
            var conventionIds = await _context.Conventions.Where(c => c.DeleteYn == "N").Select(c => c.Id).ToListAsync();
            int totalDocumentsIndexed = 0;

            foreach (var id in conventionIds)
            {
                try
                {
                    int indexedCount = await IndexConventionAsync(id);
                    totalDocumentsIndexed += indexedCount;
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index convention {ConventionId}", id);
                    result.FailureCount++;
                    result.Errors.Add($"Convention {id}: {ex.Message}");
                }
            }

            result.TotalDocumentsIndexed = totalDocumentsIndexed;
            _logger.LogInformation("Reindexing completed. Conventions processed: {Success}, Documents indexed: {TotalDocuments}", result.SuccessCount, result.TotalDocumentsIndexed);
            return result;
        }

        public async Task<int> IndexConventionAsync(int conventionId)
        {
            // Delete existing documents for this convention to ensure consistency
            await _ragService.DeleteDocumentsByMetadataAsync("convention_id", conventionId);

            var convention = await _context.Conventions
                .Include(c => c.Guests).ThenInclude(g => g.GuestAttributes)
                .Include(c => c.ScheduleTemplates).ThenInclude(st => st.ScheduleItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conventionId);

            if (convention is null) throw new ArgumentException($"Convention {conventionId} not found");

            var guestIds = convention.Guests?.Select(g => g.Id).ToList() ?? new List<int>();
            var guestSchedulesMap = new Dictionary<int, List<ScheduleItem>>();

            if (guestIds.Any())
            {
                guestSchedulesMap = await _context.GuestScheduleTemplates
                    .Where(gst => guestIds.Contains(gst.GuestId))
                    .Include(gst => gst.ScheduleTemplate).ThenInclude(st => st.ScheduleItems)
                    .AsNoTracking()
                    .GroupBy(gst => gst.GuestId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.SelectMany(gst => gst.ScheduleTemplate.ScheduleItems)
                              .OrderBy(si => si.ScheduleDate).ThenBy(si => si.StartTime)
                              .ToList()
                    );
            }

            var documentChunks = _documentBuilder.BuildDocumentChunks(convention, guestSchedulesMap);

            if (documentChunks.Any())
            {
                await _ragService.AddDocumentsAsync(documentChunks);
            }

            return documentChunks.Count;
        }
    }
}
