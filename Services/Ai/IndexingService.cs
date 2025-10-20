using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;
using LocalRAG.Services.Shared.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LocalRAG.Services.Ai;

/// <summary>
/// RAG를 위한 Convention '공용 정보' 색인을 담당하는 서비스.
/// 개인정보는 일절 다루지 않습니다.
/// </summary>
public class IndexingService
{
    private readonly IRagService _ragService;
    private readonly ILogger<IndexingService> _logger;
    private readonly ConventionDbContext _context;
    private readonly ConventionDocumentBuilder _documentBuilder;

    public IndexingService(
        IRagService ragService,
        ILogger<IndexingService> logger,
        ConventionDbContext context,
        ConventionDocumentBuilder documentBuilder)
    {
        _ragService = ragService;
        _logger = logger;
        _context = context;
        _documentBuilder = documentBuilder;
    }

    /// <summary>
    /// 모든 활성 컨벤션의 공용 정보를 다시 색인합니다.
    /// </summary>
    public async Task<IndexingResult> ReindexAllConventionsAsync()
    {
        var result = new IndexingResult();
        var conventionIds = await _context.Conventions
            .Where(c => c.DeleteYn == "N")
            .Select(c => c.Id)
            .ToListAsync();

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

    /// <summary>
    /// 특정 컨벤션의 공용 정보(기본 정보, 공용 일정 등)를 색인합니다.
    /// </summary>
    public async Task<int> IndexConventionAsync(int conventionId)
    {
        await _ragService.DeleteDocumentsByMetadataAsync("convention_id", conventionId);

        // 1.색인에 필요한 모든 '공용' 데이터를 여기서 한번에 조회합니다.
        var convention = await _context.Conventions
            .Include(c => c.Guests) // guest_summary 청크를 만들기 위해 Guests 정보는 필요합니다.
            .Include(c => c.ScheduleTemplates).ThenInclude(st => st.ScheduleItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conventionId);

        if (convention is null)
            throw new ArgumentException($"Convention {conventionId} not found");

        
        var notices = await _context.Notices
          .Where(n => n.ConventionId == conventionId && !n.IsDeleted)
          .ToListAsync();
        // (핵심 개선) 조회한 모든 데이터를 '바구니(DTO)' 객체에 담습니다.
        var indexingData = new ConventionIndexingData
        {
            Convention = convention,
            Notices = notices
            // Faqs = faqs // 나중에 FAQ가 추가되면 여기에 담기만 하면 됩니다.
        };

        var documentChunks = await _documentBuilder.BuildDocumentChunks(indexingData);

        if (documentChunks.Any())
        {
            await _ragService.AddDocumentsAsync(documentChunks);
        }

        return documentChunks.Count;
    }
}