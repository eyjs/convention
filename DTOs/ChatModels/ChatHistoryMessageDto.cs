namespace LocalRAG.DTOs.ChatModels;

public class ChatHistoryMessageDto
{
    public int userId { get; set; }
    public required string userName { get; set; }
    public string? profileImageUrl { get; set; }
    public required string message { get; set; }
    public required string createdAt { get; set; }
    public bool isAdmin { get; set; }
}
