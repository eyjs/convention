namespace LocalRAG.Models;

/// <summary>
/// 특정 참여자의 특정 액션에 대한 진행 상태
/// </summary>
public class GuestActionStatus
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 참여자 ID
    /// </summary>
    public int GuestId { get; set; }

    /// <summary>
    /// 액션 템플릿 ID
    /// </summary>
    public int ConventionActionId { get; set; }

    /// <summary>
    /// 완료 여부
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// 완료 일시
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 액션 결과 데이터 (JSON 형식)
    /// 예: 선택한 투어 ID, 설문 응답 등
    /// </summary>
    public string? ResponseDataJson { get; set; }

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Guest? Guest { get; set; }
    public ConventionAction? ConventionAction { get; set; }
}
