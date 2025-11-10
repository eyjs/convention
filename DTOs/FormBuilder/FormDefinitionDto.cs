namespace LocalRAG.DTOs.FormBuilder;

/// <summary>
/// 폼 정의 응답 DTO (순환 참조 방지)
/// </summary>
public class FormDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ConventionId { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new List<FormFieldDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 폼 필드 DTO (순환 참조 방지)
/// </summary>
public class FormFieldDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? OptionsJson { get; set; }
}
