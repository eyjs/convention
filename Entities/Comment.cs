namespace LocalRAG.Entities;

/// <summary>
/// 댓글 엔티티
/// </summary>
public class Comment
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Navigation Properties
    public virtual Notice Notice { get; set; } = null!;
    public virtual User Author { get; set; } = null!;
}
