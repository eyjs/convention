using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

/// <summary>
/// 옵션투어 정보
/// 일정의 확장 개념으로, 특정 날짜/시간에 제공되는 추가 옵션 투어
/// </summary>
public class OptionTour
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ConventionId { get; set; }

    /// <summary>
    /// 옵션투어 날짜
    /// </summary>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// 시작 시간 (예: "02:00", "09:00")
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 종료 시간 (예: "09:00", "18:30")
    /// </summary>
    [MaxLength(10)]
    public string? EndTime { get; set; }

    /// <summary>
    /// 옵션투어 이름 (예: "바뚜르산", "우붓")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 사용자 지정 옵션 ID (엑셀에서 참석자 매핑용)
    /// </summary>
    [Required]
    public int CustomOptionId { get; set; }

    /// <summary>
    /// 옵션투어 상세 내용
    /// </summary>
    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Convention? Convention { get; set; }
    public ICollection<UserOptionTour> UserOptionTours { get; set; } = new List<UserOptionTour>();
}
