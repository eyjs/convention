namespace LocalRAG.Models;

/// <summary>
/// 갤러리 (사진첩)
/// </summary>
public class Gallery
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public virtual Convention Convention { get; set; } = null!;
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<GalleryImage> Images { get; set; } = new List<GalleryImage>();
}

/// <summary>
/// 갤러리 이미지
/// </summary>
public class GalleryImage
{
    public int Id { get; set; }
    public int GalleryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int OrderNum { get; set; }
    public DateTime UploadedAt { get; set; }

    // Navigation Property
    public virtual Gallery Gallery { get; set; } = null!;
}
