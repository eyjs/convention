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
    public string? MapsTo { get; set; }
    public string? ConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderNum { get; set; }
    public string? ActionCategory { get; set; }
    public string? TargetLocation { get; set; }
    public string BehaviorType { get; set; } = string.Empty;

    /// <summary>
    /// BehaviorType=ModuleLink일 경우, 대상 모듈의 ID (예: Survey.Id)
    /// </summary>
    public int? TargetModuleId { get; set; }

    /// <summary>
    /// BehaviorType=FormBuilder일 경우, 대상 FormDefinition의 ID
    /// </summary>
    public int? TargetId { get; set; }
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
