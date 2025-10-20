using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Models;

public class Guest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ConventionId { get; set; }

    public int? UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telephone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? CorpName { get; set; }

    [MaxLength(100)]
    public string? CorpPart { get; set; }

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
    /// 비회원 접근용 고유 토큰
    /// </summary>
    [MaxLength(64)]
    public string? AccessToken { get; set; }

    /// <summary>
    /// 회원 여부 (true: User 계정 있음, false: 비회원)
    /// </summary>
    public bool IsRegisteredUser { get; set; } = false;
    public string? PasswordHash { get; set; } 
    [ForeignKey("ConventionId")]
    public virtual Convention Convention { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual ICollection<GuestScheduleTemplate> GuestScheduleTemplates { get; set; } = new List<GuestScheduleTemplate>();
    public virtual ICollection<GuestAttribute> GuestAttributes { get; set; } = new List<GuestAttribute>();

    /// <summary>
    /// 마지막으로 채팅을 읽은 시간
    /// </summary>
    public DateTime? LastChatReadTimestamp { get; set; }

    // ===== 해외 여행 정보 (PROFILE_OVERSEAS 액션용) =====
    
    /// <summary>
    /// 영문 이름 (여권상 이름)
    /// </summary>
    [MaxLength(100)]
    public string? EnglishName { get; set; }

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
    /// 비자 서류 첨부파일 ID (FileAttachment 테이블 FK)
    /// </summary>
    public int? VisaDocumentAttachmentId { get; set; }

    // Navigation Properties
    public virtual ICollection<GuestActionStatus> GuestActionStatuses { get; set; } = new List<GuestActionStatus>();
}
