namespace LocalRAG.DTOs.AdminModels;

public class SmsGuestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CorpPart { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
}
