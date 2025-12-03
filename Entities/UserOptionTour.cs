using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

/// <summary>
/// 참석자별 옵션투어 매핑
/// UserId + ConventionId + OptionTourId로 참석자가 선택한 옵션투어를 관리
/// </summary>
public class UserOptionTour
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int OptionTourId { get; set; }

    [Required]
    public int ConventionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public OptionTour? OptionTour { get; set; }
    public Convention? Convention { get; set; }
}
