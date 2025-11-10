using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities.FormBuilder;

/// <summary>
/// '폼 설계도'에 포함될 개별 필드 정의 (JSON 대신 정형화)
/// </summary>
public class FormField
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 연관된 폼 정의 ID
    /// </summary>
    public int FormDefinitionId { get; set; }

    /// <summary>
    /// 연관된 폼 정의
    /// </summary>
    public virtual FormDefinition? FormDefinition { get; set; }

    /// <summary>
    /// 필드 키 (예: "passportNo")
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 필드 라벨 (예: "여권 번호")
    /// </summary>
    [Required]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 필드 타입 (예: "text", "select", "date", "file", "repeater")
    /// </summary>
    [Required]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// 필수 여부
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Placeholder 텍스트
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// select, radio 등을 위한 옵션 (JSON: [{ "text": "A", "value": "A" }])
    /// </summary>
    public string? OptionsJson { get; set; }

    /// <summary>
    /// 유효성 검사 규칙 (선택적)
    /// </summary>
    public string? ValidationRules { get; set; }
}
