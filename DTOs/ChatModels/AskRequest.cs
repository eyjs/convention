namespace LocalRAG.DTOs.ChatModels;

public class AskRequest
{
    public string Question { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public List<ChatRequestMessage>? History { get; set; }
}
