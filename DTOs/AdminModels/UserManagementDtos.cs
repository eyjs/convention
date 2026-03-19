namespace LocalRAG.DTOs.AdminModels
{
    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class UpdateUserRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }

    public class LinkUserDto
    {
        public List<int> UserIds { get; set; } = new();
        public string? GroupName { get; set; }
    }

    public class PassportVerificationDto
    {
        public bool Verified { get; set; }
    }
}
