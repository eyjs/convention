namespace LocalRAG.DTOs.AuthModels;

public class TokenLoginRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
}
