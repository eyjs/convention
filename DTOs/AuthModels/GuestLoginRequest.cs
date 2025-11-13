namespace LocalRAG.DTOs.AuthModels;

public class GuestLoginRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int ConventionId { get; set; }
}
