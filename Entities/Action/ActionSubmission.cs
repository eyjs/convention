using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.Action;

/// <summary>
/// GenericForm 타입 액션의 응답 데이터를 저장하는 범용 테이블
/// 사용자가 제출한 폼 데이터를 JSON 형태로 저장
/// </summary>
public class ActionSubmission
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 어떤 액션(GenericForm 타입)에 대한 응답인지
    /// </summary>
    [Required]
    public int ConventionActionId { get; set; }

    /// <summary>
    /// 누가 응답했는지
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 실제 응답 데이터 (JSON 형식)
    /// 예: {"passportNo": "M12345678", "emergencyContact": "010-1234-5678"}
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string SubmissionDataJson { get; set; } = string.Empty;

    /// <summary>
    /// 최초 제출 시각
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 마지막 수정 시각
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ConventionAction ConventionAction { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
