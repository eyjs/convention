namespace LocalRAG.DTOs.ConventionModels;

public class UpdateConventionRequest
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = "DOMESTIC";
    public string? RenderType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? BrandColor { get; set; }
    public string? ThemePreset { get; set; }
    public string? ConventionImg { get; set; }
}
