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
    public string ActionType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    [JsonConverter(typeof(EmptyStringToNullDateTimeConverter))]
    public DateTime? Deadline { get; set; }
    public string MapsTo { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int OrderNum { get; set; }
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
