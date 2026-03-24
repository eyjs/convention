using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    public string? PassportImageUrl { get; set; }

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

    /// <summary>
    /// 관리자가 여권 정보를 수기 검증 완료했는지 여부
    /// User 레벨 (행사 무관, 한번 검증하면 모든 행사에서 신뢰)
    /// </summary>
    public bool PassportVerified { get; set; } = false;

    /// <summary>
    /// 여권 검증 완료 시각
    /// </summary>
    public DateTime? PassportVerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<UserConvention> UserConventions { get; set; } = new List<UserConvention>();
    public virtual ICollection<GuestAttribute> GuestAttributes { get; set; } = new List<GuestAttribute>();
    public virtual ICollection<UserActionStatus> UserActionStatuses { get; set; } = new List<UserActionStatus>();
    public virtual ICollection<GuestScheduleTemplate> GuestScheduleTemplates { get; set; } = new List<GuestScheduleTemplate>();
    public virtual ICollection<UserOptionTour> UserOptionTours { get; set; } = new List<UserOptionTour>();

    /// <summary>
    /// 이 사용자의 동반자들 (내가 주 참석자)
    /// </summary>
    [InverseProperty("User")]
    public virtual ICollection<CompanionRelation> Companions { get; set; } = new List<CompanionRelation>();

    /// <summary>
    /// 이 사용자를 동반자로 등록한 관계들 (내가 동반자)
    /// </summary>
    [InverseProperty("CompanionUser")]
    public virtual ICollection<CompanionRelation> CompanionOf { get; set; } = new List<CompanionRelation>();
}
