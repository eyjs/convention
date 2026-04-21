namespace LocalRAG.Entities;

public class AttributeCategoryItem
{
    public int Id { get; set; }
    public int AttributeCategoryId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public int OrderNum { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AttributeCategory? AttributeCategory { get; set; }
}
