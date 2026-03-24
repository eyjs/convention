using System.ComponentModel.DataAnnotations;
using LocalRAG.DTOs.ScheduleModels;

namespace LocalRAG.Entities;

/// <summary>
/// 일정/옵션투어 이미지 갤러리
/// </summary>
public class ScheduleImage
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 일정 항목 FK (nullable)
    /// </summary>
    public int? ScheduleItemId { get; set; }

    /// <summary>
    /// 옵션투어 FK (nullable)
    /// </summary>
    public int? OptionTourId { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    public int OrderNum { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ScheduleItem? ScheduleItem { get; set; }
    public OptionTour? OptionTour { get; set; }
}
