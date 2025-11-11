using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.UserModels;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "현재 비밀번호는 필수입니다.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "새 비밀번호는 필수입니다.")]
    [MinLength(6, ErrorMessage = "비밀번호는 최소 6자 이상이어야 합니다.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호 확인은 필수입니다.")]
    [Compare("NewPassword", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
