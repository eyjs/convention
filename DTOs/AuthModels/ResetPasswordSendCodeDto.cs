namespace LocalRAG.DTOs.AuthModels;

public class ResetPasswordSendCodeDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
