namespace LocalRAG.DTOs.AuthModels;

public class ResetPasswordVerifyDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
