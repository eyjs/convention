using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string LoginId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Guest";

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    public bool PhoneVerified { get; set; } = false;

    [MaxLength(256)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(512)]
    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();
}
