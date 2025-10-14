namespace LocalRAG.Models;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceInfo> Sources { get; set; } = new();
    public string LlmProvider { get; set; } = string.Empty;
    public string? Intent { get; set; }
}

public class SourceInfo
{
    public string? Content { get; set; }
    public float Similarity { get; set; }
    public string? Type { get; set; }
    public int? ConventionId { get; set; }
    public string? ConventionTitle { get; set; }
}

public class ChatUserContext
{
    public UserRole Role { get; set; }
    public int? GuestId { get; set; }
    public string? MemberId { get; set; }
}

public enum UserRole
{
    Guest,
    Admin,
    Anonymous
}
