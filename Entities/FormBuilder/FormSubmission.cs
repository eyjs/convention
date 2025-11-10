using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.FormBuilder;

/// <summary>
/// 사용자가 '폼 빌더'를 통해 제출한 '응답 데이터'
/// </summary>
public class FormSubmission
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 어떤 폼에 대한 응답인지
    /// </summary>
    public int FormDefinitionId { get; set; }

    /// <summary>
    /// 연관된 폼 정의
    /// </summary>
    public virtual FormDefinition? FormDefinition { get; set; }

    /// <summary>
    /// 누가 응답했는지 (사용자 ID)
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 연관된 사용자
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// [핵심] 폼 빌더를 통해 수집된 실제 데이터 (JSON)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string SubmissionDataJson { get; set; } = string.Empty;

    /// <summary>
    /// 제출일
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
