namespace LocalRAG.Services;

public enum UserRole
{
    Admin,
    Guest
}

public class ChatUserContext
{
    public UserRole Role { get; set; }
    public int? GuestId { get; set; }
    public string? MemberId { get; set; }
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceInfo> Sources { get; set; } = new();
    public string LlmProvider { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class SourceInfo
{
    public string Content { get; set; } = string.Empty;
    public float Similarity { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? ConventionId { get; set; }
    public string? ConventionTitle { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

// 👇 --- 여기가 수정된 부분입니다 --- 👇
public class IndexingResult
{
    /// <summary>
    /// 성공적으로 처리된 행사의 수
    /// </summary>
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 실제 VectorStore에 색인된 총 문서(행사+참석자+일정 등)의 수
    /// </summary>
    public int TotalDocumentsIndexed { get; set; }
    public int TotalCount => SuccessCount + FailureCount;
}