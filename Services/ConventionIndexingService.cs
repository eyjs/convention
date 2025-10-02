using LocalRAG.Interfaces;
using LocalRAG.Models.Convention;
using LocalRAG.Repositories;
using System.Text;

namespace LocalRAG.Services;

/// <summary>
/// Convention 데이터 색인화 서비스
/// 
/// 주요 기능:
/// 1. Convention 데이터를 텍스트로 변환
/// 2. 임베딩 생성 및 VectorStore에 저장
/// 3. 전체 재색인 및 증분 업데이트
/// </summary>
public class ConventionIndexingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRagService _ragService;
    private readonly ILogger<ConventionIndexingService> _logger;

    public ConventionIndexingService(
        IUnitOfWork unitOfWork,
        IRagService ragService,
        ILogger<ConventionIndexingService> logger)
    {
        _unitOfWork = unitOfWork;
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// 전체 Convention 데이터를 재색인합니다.
    /// </summary>
    public async Task<IndexingResult> ReindexAllConventionsAsync()
    {
        var result = new IndexingResult();
        var conventions = await _unitOfWork.Conventions.GetActiveConventionsAsync();

        foreach (var convention in conventions)
        {
            try
            {
                await IndexConventionAsync(convention);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index convention {ConventionId}", convention.Id);
                result.FailureCount++;
                result.Errors.Add($"Convention {convention.Id}: {ex.Message}");
            }
        }

        _logger.LogInformation(
            "Reindexing completed. Success: {Success}, Failures: {Failures}",
            result.SuccessCount, result.FailureCount);

        return result;
    }

    /// <summary>
    /// 특정 Convention을 색인합니다.
    /// </summary>
    public async Task<string> IndexConventionAsync(int conventionId)
    {
        var convention = await _unitOfWork.Conventions
            .GetConventionWithDetailsAsync(conventionId);

        if (convention == null)
        {
            throw new ArgumentException($"Convention {conventionId} not found");
        }

        return await IndexConventionAsync(convention);
    }

    /// <summary>
    /// Convention 엔티티를 색인합니다.
    /// </summary>
    private async Task<string> IndexConventionAsync(Convention convention)
    {
        // 1. Convention 기본 정보 색인
        var conventionText = BuildConventionText(convention);
        var metadata = new Dictionary<string, object>
        {
            ["convention_id"] = convention.Id,
            ["type"] = "convention",
            ["title"] = convention.Title,
            ["convention_type"] = convention.ConventionType
        };

        var documentId = await _ragService.AddDocumentAsync(conventionText, metadata);

        // 2. 참석자 정보 색인
        if (convention.Guests?.Any() == true)
        {
            foreach (var guest in convention.Guests)
            {
                await IndexGuestAsync(guest, convention.Id);
            }
        }

        // 3. 일정 정보 색인
        if (convention.Schedules?.Any() == true)
        {
            foreach (var schedule in convention.Schedules)
            {
                await IndexScheduleAsync(schedule, convention.Id);
            }
        }

        // 4. 메뉴 정보 색인
        if (convention.Menus?.Any() == true)
        {
            foreach (var menu in convention.Menus)
            {
                await IndexMenuAsync(menu, convention.Id);
            }
        }

        return documentId;
    }

    /// <summary>
    /// Guest 정보를 색인합니다.
    /// </summary>
    private async Task<string> IndexGuestAsync(Guest guest, int conventionId)
    {
        var text = BuildGuestText(guest);
        var metadata = new Dictionary<string, object>
        {
            ["convention_id"] = conventionId,
            ["guest_id"] = guest.Id,
            ["type"] = "guest",
            ["name"] = guest.GuestName
        };

        return await _ragService.AddDocumentAsync(text, metadata);
    }

    /// <summary>
    /// Schedule 정보를 색인합니다.
    /// </summary>
    private async Task<string> IndexScheduleAsync(Schedule schedule, int conventionId)
    {
        var text = BuildScheduleText(schedule);
        var metadata = new Dictionary<string, object>
        {
            ["convention_id"] = conventionId,
            ["schedule_id"] = schedule.Id,
            ["type"] = "schedule",
            ["date"] = schedule.ScheduleDate.ToString("yyyy-MM-dd")
        };

        return await _ragService.AddDocumentAsync(text, metadata);
    }

    /// <summary>
    /// Menu 정보를 색인합니다.
    /// </summary>
    private async Task<string> IndexMenuAsync(Menu menu, int conventionId)
    {
        var text = BuildMenuText(menu);
        var metadata = new Dictionary<string, object>
        {
            ["convention_id"] = conventionId,
            ["menu_id"] = menu.Id,
            ["type"] = "menu"
        };

        return await _ragService.AddDocumentAsync(text, metadata);
    }

    // ============================================================
    // 텍스트 생성 메서드들
    // ============================================================

    /// <summary>
    /// Convention 정보를 검색 가능한 텍스트로 변환합니다.
    /// </summary>
    private string BuildConventionText(Convention convention)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"행사명: {convention.Title}");
        sb.AppendLine($"행사 유형: {convention.ConventionType}");
        
        if (convention.StartDate.HasValue)
        {
            sb.AppendLine($"시작일: {convention.StartDate.Value:yyyy년 MM월 dd일}");
        }
        
        if (convention.EndDate.HasValue)
        {
            sb.AppendLine($"종료일: {convention.EndDate.Value:yyyy년 MM월 dd일}");
        }

        // 담당자 정보
        if (convention.Owners?.Any() == true)
        {
            sb.AppendLine("\n담당자 정보:");
            foreach (var owner in convention.Owners)
            {
                sb.AppendLine($"- {owner.Name} ({owner.Telephone})");
            }
        }

        // 기능 정보
        if (convention.Features?.Any() == true)
        {
            var enabledFeatures = convention.Features
                .Where(f => f.IsEnabled == "Y")
                .Select(f => f.FeatureName);
            
            if (enabledFeatures.Any())
            {
                sb.AppendLine($"\n활성화된 기능: {string.Join(", ", enabledFeatures)}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Guest 정보를 검색 가능한 텍스트로 변환합니다.
    /// </summary>
    private string BuildGuestText(Guest guest)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"참석자명: {guest.GuestName}");
        
        if (!string.IsNullOrEmpty(guest.CorpHrId))
        {
            sb.AppendLine($"사번: {guest.CorpHrId}");
        }
        
        if (!string.IsNullOrEmpty(guest.CorpPart))
        {
            sb.AppendLine($"부서: {guest.CorpPart}");
        }

        // 속성 정보
        if (guest.Attributes?.Any() == true)
        {
            sb.AppendLine("\n추가 정보:");
            foreach (var attr in guest.Attributes)
            {
                sb.AppendLine($"- {attr.AttributeKey}: {attr.AttributeValue}");
            }
        }

        // 동반자 정보
        if (guest.Companions?.Any() == true)
        {
            sb.AppendLine("\n동반자:");
            foreach (var companion in guest.Companions)
            {
                sb.AppendLine($"- {companion.Name} ({companion.Relation})");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Schedule 정보를 검색 가능한 텍스트로 변환합니다.
    /// </summary>
    private string BuildScheduleText(Schedule schedule)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"일정: {schedule.Name}");
        sb.AppendLine($"날짜: {schedule.ScheduleDate:yyyy년 MM월 dd일}");

        if (schedule.StartTime.HasValue)
        {
            sb.AppendLine($"시작 시간: {schedule.StartTime.Value:hh\\:mm}");
        }

        if (schedule.EndTime.HasValue)
        {
            sb.AppendLine($"종료 시간: {schedule.EndTime.Value:hh\\:mm}");
        }

        if (!string.IsNullOrEmpty(schedule.Group))
        {
            sb.AppendLine($"그룹: {schedule.Group}");
        }

        if (!string.IsNullOrEmpty(schedule.Description))
        {
            sb.AppendLine($"설명: {schedule.Description}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Menu 정보를 검색 가능한 텍스트로 변환합니다.
    /// </summary>
    private string BuildMenuText(Menu menu)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"메뉴: {menu.ItemName ?? "제목 없음"}");

        if (menu.Sections?.Any() == true)
        {
            sb.AppendLine("\n섹션:");
            foreach (var section in menu.Sections.OrderBy(s => s.OrderNum))
            {
                if (!string.IsNullOrEmpty(section.Title))
                {
                    sb.AppendLine($"\n## {section.Title}");
                }
                
                if (!string.IsNullOrEmpty(section.Contents))
                {
                    sb.AppendLine(section.Contents);
                }
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// 색인 결과
/// </summary>
public class IndexingResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public int TotalCount => SuccessCount + FailureCount;
}
