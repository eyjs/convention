namespace LocalRAG.Entities;

public class AttributeCategory
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; } // 이모지 또는 아이콘명
    public int OrderNum { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Convention? Convention { get; set; }
    public List<AttributeCategoryItem> Items { get; set; } = new();
}
