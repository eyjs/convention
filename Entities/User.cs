using System.ComponentModel.DataAnnotations;
using LocalRAG.DTOs.ScheduleModels;

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

    // ===== Guest 엔티티로부터 병합된 속성들 =====

    /// <summary>
    /// 회사명
    /// </summary>
    [MaxLength(100)]
    public string? CorpName { get; set; }

    /// <summary>
    /// 부서
    /// </summary>
    [MaxLength(100)]
    public string? CorpPart { get; set; }

    /// <summary>
    /// 비고 (관리자용 메모)
    /// </summary>
    [MaxLength(500)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 주민등록번호 (앞 6자리 또는 전체, 암호화 권장)
    /// </summary>
    [MaxLength(50)]
    public string? ResidentNumber { get; set; }

    /// <summary>
    /// 소속 정보 (통계/라벨용)
    /// </summary>
    [MaxLength(100)]
    public string? Affiliation { get; set; }

    /// <summary>
    /// 영문 이름 (여권상 이름) - Deprecated, use FirstName and LastName
    /// </summary>
    [MaxLength(100)]
    public string? EnglishName { get; set; }

    /// <summary>
    /// 영문 이름 (First Name)
    /// </summary>
    [MaxLength(50)]
    public string? FirstName { get; set; }

    /// <summary>
    /// 영문 성 (Last Name)
    /// </summary>
    [MaxLength(50)]
    public string? LastName { get; set; }

    /// <summary>
    /// 여권 번호
    /// </summary>
    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    /// <summary>
    /// 여권 만료일
    /// </summary>
    public DateOnly? PassportExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<UserConvention> UserConventions { get; set; } = new List<UserConvention>();
    public virtual ICollection<GuestAttribute> GuestAttributes { get; set; } = new List<GuestAttribute>();
    public virtual ICollection<UserActionStatus> UserActionStatuses { get; set; } = new List<UserActionStatus>();
    public virtual ICollection<GuestScheduleTemplate> GuestScheduleTemplates { get; set; } = new List<GuestScheduleTemplate>();
}
