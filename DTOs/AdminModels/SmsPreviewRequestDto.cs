namespace LocalRAG.DTOs.AdminModels;

public class SmsPreviewRequestDto
{
    public string Content { get; set; } = string.Empty;
    public int TargetUserId { get; set; }
}
