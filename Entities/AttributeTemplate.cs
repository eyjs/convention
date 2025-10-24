namespace LocalRAG.Entities;

/// <summary>
/// 속성 템플릿 (행사별 사전정의된 속성)
/// </summary>
public class AttributeTemplate
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public string? AttributeValues { get; set; } // JSON 배열 형태: ["105", "100", "95"]
    public int OrderNum { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Convention? Convention { get; set; }
}
