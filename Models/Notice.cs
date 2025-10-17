namespace LocalRAG.Models;

/// <summary>
/// 공지사항 엔티티
/// </summary>
public class Notice
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int AuthorId { get; set; }
    public int ConventionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Navigation Properties
    public virtual User Author { get; set; } = null!;
    public virtual Convention Convention { get; set; } = null!;
    public virtual ICollection<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

/// <summary>
/// 파일 첨부 엔티티
/// </summary>
public class FileAttachment
{
    public int Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string SavedName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // notice, board, gallery
    public int? NoticeId { get; set; }
    public int? BoardPostId { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation Properties
    public virtual Notice? Notice { get; set; }
}
