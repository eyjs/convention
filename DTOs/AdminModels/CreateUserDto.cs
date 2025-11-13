namespace LocalRAG.DTOs.AdminModels;

public class CreateUserDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Guest";
}
