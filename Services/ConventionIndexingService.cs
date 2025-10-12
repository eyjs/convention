using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Repositories;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;

namespace LocalRAG.Services;

public class ConventionIndexingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRagService _ragService;
    private readonly ILogger<ConventionIndexingService> _logger;
    private readonly ConventionDbContext _context;

    public ConventionIndexingService(IUnitOfWork unitOfWork, IRagService ragService, ILogger<ConventionIndexingService> logger, ConventionDbContext context)
    {
        _unitOfWork = unitOfWork;
        _ragService = ragService;
        _logger = logger;
        _context = context;
    }

    public async Task<IndexingResult> ReindexAllConventionsAsync()
    {
        var result = new IndexingResult();
        var conventions = await _unitOfWork.Conventions.GetActiveConventionsAsync();
        int totalDocumentsIndexed = 0;

        foreach (var convention in conventions)
        {
            try
            {
                int indexedCount = await IndexConventionAsync(convention.Id);
                totalDocumentsIndexed += indexedCount;
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index convention {ConventionId}", convention.Id);
                result.FailureCount++;
                result.Errors.Add($"Convention {convention.Id}: {ex.Message}");
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
            .Include(c => c.Owners)
            .Include(c => c.Guests).ThenInclude(g => g.GuestAttributes)
            .Include(c => c.ScheduleTemplates).ThenInclude(st => st.ScheduleItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conventionId);

        if (convention is null)
        {
            throw new ArgumentException($"Convention {conventionId} not found");
        }

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
        sb.AppendLine($"- 행사 유형: {(convention.ConventionType == "OVERSEAS" ? "해외" : "국내")} 행사");
        if (convention.StartDate.HasValue) sb.AppendLine($"- 시작일: {convention.StartDate.Value:yyyy년 MM월 dd일}");
        if (convention.EndDate.HasValue) sb.AppendLine($"- 종료일: {convention.EndDate.Value:yyyy년 MM월 dd일}");
        if (convention.Owners?.Any() == true)
        {
            sb.AppendLine("- 담당자:");
            foreach (var owner in convention.Owners)
            {
                sb.AppendLine($"  - {owner.Name} ({owner.Telephone})");
            }
        }
        return sb.ToString();
    }

    private string BuildGuestText(Guest guest, List<ScheduleItem> assignedItems)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# 참석자 정보: {guest.GuestName}");
        sb.AppendLine($"- 참석자 ID(GuestId): {guest.Id}");
        sb.AppendLine($"- 이름: {guest.GuestName}");
        if (!string.IsNullOrEmpty(guest.CorpPart)) sb.AppendLine($"- 부서: {guest.CorpPart}");
        if (!string.IsNullOrEmpty(guest.Telephone)) sb.AppendLine($"- 연락처: {guest.Telephone}");
        if (guest.GuestAttributes?.Any() == true)
        {
            sb.AppendLine("- 추가 속성:");
            foreach (var attr in guest.GuestAttributes)
            {
                sb.AppendLine($"  - {attr.AttributeKey}: {attr.AttributeValue}");
            }
        }

        if (assignedItems.Any())
        {
            sb.AppendLine("\n## 할당된 전체 일정");
            foreach (var item in assignedItems)
            {
                sb.AppendLine($"- {item.ScheduleDate:yyyy-MM-dd} {item.StartTime}: {item.Title}{(string.IsNullOrEmpty(item.Location) ? "" : $" (장소: {item.Location})")}");
            }
        }
        else
        {
            sb.AppendLine("\n## 할당된 전체 일정: 없음");
        }
        return sb.ToString();
    }
}