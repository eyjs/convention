namespace LocalRAG.DTOs.UserModels;

public class UserProfileDto
{
    public int Id { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportExpiryDate { get; set; } // yyyy-MM-dd format
    public string? Affiliation { get; set; }
    public string? CorpName { get; set; }
    public string? CorpPart { get; set; }
    public string? ProfileImageUrl { get; set; }
}
