using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities.FormBuilder;

/// <summary>
/// 관리자가 생성하는 '폼 설계도' 마스터
/// </summary>
public class FormDefinition
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 폼 이름 (예: "VIP 사전 등록 폼")
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 폼 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 연관된 행사 ID
    /// </summary>
    public int ConventionId { get; set; }

    /// <summary>
    /// 연관된 행사
    /// </summary>
    public virtual Convention? Convention { get; set; }

    /// <summary>
    /// 폼에 포함된 필드 목록
    /// </summary>
    public virtual ICollection<FormField> Fields { get; set; } = new List<FormField>();

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
