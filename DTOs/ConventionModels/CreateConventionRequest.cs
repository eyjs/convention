namespace LocalRAG.DTOs.ConventionModels;

public class CreateConventionRequest
{
    public string Title { get; set; } = string.Empty;
    public string ConventionType { get; set; } = "DOMESTIC";
    public string? RenderType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? BrandColor { get; set; }
    public string? ThemePreset { get; set; }
    public string? ConventionImg { get; set; }
    public string? Location { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationCountryCode { get; set; }
    public string? EmergencyContactsJson { get; set; }
    public string? MeetingPointInfo { get; set; }
}
