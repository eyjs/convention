namespace LocalRAG.DTOs.AdminModels;

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CorpPart { get; set; }
    public string? ResidentNumber { get; set; }
    public string? Affiliation { get; set; }
    public string? Password { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}
