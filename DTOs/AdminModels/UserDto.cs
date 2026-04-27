namespace LocalRAG.DTOs.AdminModels;

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CorpPart { get; set; }
    public string? ResidentNumber { get; set; }
    public string? Affiliation { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportExpiryDate { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    /// <summary>
    /// 속성 저장 시 행사 격리를 위한 ConventionId (선택)
    /// </summary>
    public int? ConventionId { get; set; }
}
