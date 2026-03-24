using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities;

/// <summary>
/// 참석자 간 동반자 관계
/// 주 참석자(User)가 1:N 동반자(Companion)를 가질 수 있음
/// 동반자도 User 엔티티이며, 관계 유형(RelationType)으로 관계를 식별
/// </summary>
public class CompanionRelation
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 행사 ID (동반자 관계는 행사 단위)
    /// </summary>
    [Required]
    public int ConventionId { get; set; }

    /// <summary>
    /// 주 참석자 ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 동반자 ID (역시 User)
    /// </summary>
    [Required]
    public int CompanionUserId { get; set; }

    /// <summary>
    /// 관계 유형 (예: "배우자", "자녀", "부모", "동료" 등)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RelationType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ConventionId")]
    public virtual Convention Convention { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("CompanionUserId")]
    public virtual User CompanionUser { get; set; } = null!;
}
