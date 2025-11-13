namespace LocalRAG.DTOs.ConventionModels;

public class AttributeDefinitionDto
{
    public string AttributeKey { get; set; } = string.Empty;
    public string? Options { get; set; }
    public int OrderNum { get; set; }
    public bool IsRequired { get; set; }
}
