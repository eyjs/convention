using LocalRAG.Interfaces;
using LocalRAG.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;

namespace LocalRAG.Services;

public class ConventionIndexingService
{
    private readonly IRagService _ragService;
    private readonly ILogger<ConventionIndexingService> _logger;
    private readonly ConventionDbContext _context;

    public ConventionIndexingService(IRagService ragService, ILogger<ConventionIndexingService> logger, ConventionDbContext context)
    {
        _ragService = ragService;
        _logger = logger;
        _context = context;
    }

    public async Task<IndexingResult> ReindexAllConventionsAsync()
    {
        var result = new IndexingResult();
        var conventions = await _context.Conventions.Where(c => c.DeleteYn == "N").Select(c => c.Id).ToListAsync();
        int totalDocumentsIndexed = 0;

        foreach (var id in conventions)
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
        int documentCount = 0;
        var convention = await _context.Conventions
            .Include(c => c.Guests).ThenInclude(g => g.GuestAttributes)
            .Include(c => c.ScheduleTemplates).ThenInclude(st => st.ScheduleItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conventionId);

        if (convention is null) throw new ArgumentException($"Convention {conventionId} not found");

        var conventionText = BuildConventionText(convention);
        var conventionMetadata = new Dictionary<string, object> { { "type", "convention" }, { "convention_id", convention.Id }, { "title", convention.Title } };
        await _ragService.AddDocumentAsync(conventionText, conventionMetadata);
        documentCount++;

        if (convention.Guests is not null)
        {
            var allTemplates = convention.ScheduleTemplates.ToDictionary(st => st.Id);
            foreach (var guest in convention.Guests)
            {
                var guestTemplates = await _context.GuestScheduleTemplates.Where(gst => gst.GuestId == guest.Id).Select(gst => gst.ScheduleTemplateId).ToListAsync();
                var assignedItems = allTemplates.Where(kvp => guestTemplates.Contains(kvp.Key)).SelectMany(kvp => kvp.Value.ScheduleItems).OrderBy(si => si.ScheduleDate).ThenBy(si => si.StartTime).ToList();

                var guestText = BuildGuestText(guest, assignedItems);
                var guestMetadata = new Dictionary<string, object> { { "type", "guest" }, { "convention_id", convention.Id }, { "guest_id", guest.Id }, { "name", guest.GuestName } };
                await _ragService.AddDocumentAsync(guestText, guestMetadata);
                documentCount++;
            }
        }
        return documentCount;
    }

    private string BuildConventionText(Convention convention)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# 행사 정보: {convention.Title}");
        sb.AppendLine($"- 행사 ID: {convention.Id}");
        return sb.ToString();
    }

    private string BuildGuestText(Guest guest, List<ScheduleItem> assignedItems)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# 참석자 정보: {guest.GuestName}");
        sb.AppendLine($"- 참석자 이름: {guest.GuestName}");
        sb.AppendLine($"- 참석자 ID: {guest.Id}");

        if (assignedItems.Any())
        {
            sb.AppendLine("\n## 할당된 전체 일정:");
            foreach (var item in assignedItems)
            {
                sb.AppendLine($"- {item.ScheduleDate:yyyy-MM-dd} {item.StartTime}: {item.Title}");
            }
        }
        else
        {
            sb.AppendLine("\n## 할당된 전체 일정: 없음");
        }
        return sb.ToString();
    }
}