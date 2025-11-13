namespace LocalRAG.DTOs.ConventionModels;

public class AttributeTemplateDto
{
    public string AttributeKey { get; set; } = string.Empty;
    public string? AttributeValues { get; set; }
    public int OrderNum { get; set; }
}
