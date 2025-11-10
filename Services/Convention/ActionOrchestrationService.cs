using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.ActionModels;
using LocalRAG.Entities.Action;
using LocalRAG.Interfaces;

namespace LocalRAG.Services.Convention;

/// <summary>
/// 액션 오케스트레이터 서비스
/// 다양한 BehaviorType의 액션들을 통합하여 일관된 인터페이스 제공
/// </summary>
public class ActionOrchestrationService : IActionOrchestrationService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ActionOrchestrationService> _logger;
    private readonly HttpClient _httpClient;

    public ActionOrchestrationService(
        ConventionDbContext context,
        ILogger<ActionOrchestrationService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<OrchestratedActionDto>> GetUserActionsAsync(int conventionId, int userId)
    {
        // 1. 행사의 모든 활성 액션 조회
        var actions = await _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId && a.IsActive)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.OrderNum)
            .ToListAsync();

        var result = new List<OrchestratedActionDto>();

        foreach (var action in actions)
        {
            var dto = await BuildOrchestratedActionDto(action, userId);
            result.Add(dto);
        }

        return result;
    }

    public async Task<OrchestratedActionDto?> GetUserActionAsync(int actionId, int userId)
    {
        var action = await _context.ConventionActions
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == actionId);

        if (action == null)
            return null;

        return await BuildOrchestratedActionDto(action, userId);
    }

    /// <summary>
    /// ConventionAction과 사용자 ID를 기반으로 OrchestratedActionDto 생성
    /// BehaviorType에 따라 다른 소스에서 상태 정보를 가져옴
    /// </summary>
    private async Task<OrchestratedActionDto> BuildOrchestratedActionDto(ConventionAction action, int userId)
    {
        var dto = new OrchestratedActionDto
        {
            Id = action.Id,
            Title = action.Title,
            Description = action.Description,
            Deadline = action.Deadline,
            IsRequired = action.IsRequired,
            Category = action.Category ?? action.Template?.Category,
            IconClass = action.IconClass ?? action.Template?.IconClass,
            ActionCategory = action.ActionCategory,
            TargetLocation = action.TargetLocation,
            BehaviorType = action.BehaviorType.ToString(),
            OrderNum = action.OrderNum,
            Route = CalculateRoute(action),
            Status = "NotStarted",
            Summary = null,
            CompletedAt = null
        };

        // BehaviorType에 따라 상태 및 요약 정보 수집
        switch (action.BehaviorType)
        {
            case ActionBehaviorType.StatusOnly:
                await PopulateStatusOnlyData(dto, action.Id, userId);
                break;

            case ActionBehaviorType.FormBuilder:
                await PopulateFormBuilderData(dto, action.TargetId, userId);
                break;

            case ActionBehaviorType.ModuleLink:
                await PopulateModuleLinkData(dto, action.ModuleIdentifier, action.TargetId, userId);
                break;

            case ActionBehaviorType.Link:
                // Link는 클릭만 하면 되므로 StatusOnly와 동일
                await PopulateStatusOnlyData(dto, action.Id, userId);
                break;

            default:
                _logger.LogWarning("Unknown BehaviorType: {BehaviorType} for action {ActionId}", action.BehaviorType, action.Id);
                break;
        }

        return dto;
    }

    /// <summary>
    /// StatusOnly 타입: UserActionStatus 테이블 조회
    /// </summary>
    private async Task PopulateStatusOnlyData(OrchestratedActionDto dto, int actionId, int userId)
    {
        var status = await _context.UserActionStatuses
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (status != null && status.IsComplete)
        {
            dto.Status = "Completed";
            dto.Summary = "완료됨";
            dto.CompletedAt = status.CompletedAt;
        }
        else
        {
            dto.Status = "NotStarted";
            dto.Summary = "미완료";
        }
    }

    /// <summary>
    /// FormBuilder 타입: FormSubmissions 테이블 직접 조회
    /// </summary>
    private async Task PopulateFormBuilderData(OrchestratedActionDto dto, int? formDefinitionId, int userId)
    {
        if (!formDefinitionId.HasValue)
        {
            dto.Status = "NotStarted";
            dto.Summary = "폼 설정 오류";
            return;
        }

        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId.Value && s.UserId == userId);

        if (submission != null)
        {
            dto.Status = "Completed";
            dto.Summary = "제출 완료";
            dto.CompletedAt = submission.SubmittedAt;
        }
        else
        {
            dto.Status = "NotStarted";
            dto.Summary = "미제출";
        }
    }

    /// <summary>
    /// ModuleLink 타입: 표준화 계약 API 호출
    /// </summary>
    private async Task PopulateModuleLinkData(OrchestratedActionDto dto, string? moduleIdentifier, int? targetId, int userId)
    {
        if (string.IsNullOrEmpty(moduleIdentifier) || !targetId.HasValue)
        {
            dto.Status = "NotStarted";
            dto.Summary = "모듈 설정 오류";
            return;
        }

        try
        {
            // 모듈별 API 경로 결정
            string apiBasePath = DetermineModuleApiPath(moduleIdentifier);

            // 표준화 계약 API 호출: /status 와 /summary
            var statusUrl = $"{apiBasePath}/{targetId}/status";
            var summaryUrl = $"{apiBasePath}/{targetId}/summary";

            // HttpContext에서 Authorization 헤더를 가져오려면 IHttpContextAccessor 필요
            // 현재는 내부 DB 조회로 대체 (같은 서버 내부 호출)

            // Survey 모듈의 경우 직접 DB 조회로 구현
            if (moduleIdentifier == "Survey")
            {
                await PopulateSurveyModuleData(dto, targetId.Value, userId);
            }
            else
            {
                // 향후 다른 모듈 추가 시 확장 가능
                dto.Status = "NotStarted";
                dto.Summary = $"지원되지 않는 모듈: {moduleIdentifier}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ModuleLink 데이터 조회 중 오류 발생: {ModuleIdentifier}, TargetId={TargetId}", moduleIdentifier, targetId);
            dto.Status = "NotStarted";
            dto.Summary = "조회 오류";
        }
    }

    /// <summary>
    /// Survey 모듈의 상태 조회 (내부 DB 조회)
    /// </summary>
    private async Task PopulateSurveyModuleData(OrchestratedActionDto dto, int surveyId, int userId)
    {
        // Survey 테이블 조회
        var survey = await _context.Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null)
        {
            dto.Status = "NotStarted";
            dto.Summary = "설문을 찾을 수 없음";
            return;
        }

        // SurveyResponse 조회
        var response = await _context.SurveyResponses
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.UserId == userId);

        if (response != null)
        {
            dto.Status = "Completed";
            var totalQuestions = survey.Questions?.Count ?? 0;
            var answeredQuestions = response.Details?.Count ?? 0;
            dto.Summary = $"{answeredQuestions}/{totalQuestions} 항목 응답 완료";
            dto.CompletedAt = response.SubmittedAt;
        }
        else
        {
            dto.Status = "NotStarted";
            dto.Summary = "미응답";
        }
    }

    /// <summary>
    /// ModuleIdentifier에 따른 API 경로 결정
    /// </summary>
    private string DetermineModuleApiPath(string moduleIdentifier)
    {
        return moduleIdentifier switch
        {
            "Survey" => "/api/surveys",
            "SeatMap" => "/api/seatmaps", // 향후 추가
            _ => throw new NotSupportedException($"지원되지 않는 모듈: {moduleIdentifier}")
        };
    }

    /// <summary>
    /// ConventionAction으로부터 프론트엔드 라우트 경로 계산
    /// </summary>
    private string CalculateRoute(ConventionAction action)
    {
        return action.BehaviorType switch
        {
            ActionBehaviorType.FormBuilder when action.TargetId.HasValue
                => $"/feature/form/{action.TargetId}",

            ActionBehaviorType.ModuleLink when !string.IsNullOrEmpty(action.FrontendRoute) && action.TargetId.HasValue
                => $"{action.FrontendRoute.TrimEnd('/')}/{action.TargetId}",

            ActionBehaviorType.ModuleLink
                => action.MapsTo,

            ActionBehaviorType.Link
                => action.MapsTo,

            ActionBehaviorType.StatusOnly
                => action.MapsTo,

            _ => action.MapsTo
        };
    }
}
