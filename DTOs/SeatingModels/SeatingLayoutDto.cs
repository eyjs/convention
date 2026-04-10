namespace LocalRAG.DTOs.SeatingModels;

public class SeatingLayoutDto
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }
    public string LayoutJson { get; set; } = "{\"tables\":[],\"decors\":[],\"lines\":[]}";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SeatingLayoutListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public int TableCount { get; set; }
    public int AssignedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSeatingLayoutRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CanvasWidth { get; set; } = 4000;
    public int CanvasHeight { get; set; } = 3000;
}

public class UpdateSeatingLayoutRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? CanvasWidth { get; set; }
    public int? CanvasHeight { get; set; }
    public string? LayoutJson { get; set; }
    public string? BackgroundImageUrl { get; set; }
}
