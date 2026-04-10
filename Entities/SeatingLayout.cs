namespace LocalRAG.Entities;

/// <summary>
/// 좌석 배치도 (식당/연회장 도식도)
/// </summary>
public class SeatingLayout
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public int CanvasWidth { get; set; } = 4000;
    public int CanvasHeight { get; set; } = 3000;

    /// <summary>
    /// 도식도 데이터 JSON
    /// 구조: { tables: [...], decors: [...], lines: [...] }
    /// </summary>
    public string LayoutJson { get; set; } = "{\"tables\":[],\"decors\":[],\"lines\":[]}";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public virtual Convention Convention { get; set; } = null!;
}
