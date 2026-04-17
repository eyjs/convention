namespace LocalRAG.Entities;

public class HomeBanner
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkLabel { get; set; }
    public string? Title { get; set; }
    /// <summary>
    /// 상세 페이지 이미지 URL 배열 (JSON). null이면 상세 페이지 없음
    /// </summary>
    public string? DetailImagesJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
