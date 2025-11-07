using System.Text.Json.Serialization;
using LocalRAG.Utilities;

namespace LocalRAG.DTOs.ActionModels;

/// <summary>
/// ConventionAction 생성/수정 요청 DTO
/// </summary>
public class ConventionActionDto
{
    public int? Id { get; set; }
    public int ConventionId { get; set; }
    public string Title { get; set; } = string.Empty;
    [JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    public DateTime? Deadline { get; set; }
    public string MapsTo { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderNum { get; set; }

    /// <summary>
    /// 액션 카테고리 (UI 렌더링 타입: BUTTON, MENU, AUTO_POPUP, BANNER, CARD)
    /// null이면 체크리스트 전용 액션
    /// </summary>
    public string? ActionCategory { get; set; }

    /// <summary>
    /// 타겟 위치 (UI 렌더링 위치: HOME_SUB_HEADER, SCHEDULE_CONTENT_TOP 등)
    /// </summary>
    public string? TargetLocation { get; set; }

    /// <summary>
    /// 액션 실행 방식 (StatusOnly, GenericForm, ModuleLink, Link)
    /// </summary>
    public LocalRAG.Entities.Action.ActionBehaviorType BehaviorType { get; set; }

    /// <summary>
    /// ModuleLink 타입일 경우, 대상 모듈의 PK ID (예: SurveyId)
    /// </summary>
    public int? TargetModuleId { get; set; }
}

/// <summary>
/// 여행 정보 제출 요청 DTO (PROFILE_OVERSEAS 액션용)
/// </summary>
public class TravelInfoDto
{
    public string EnglishName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateOnly PassportExpiryDate { get; set; }
    public int? VisaDocumentAttachmentId { get; set; }
}
