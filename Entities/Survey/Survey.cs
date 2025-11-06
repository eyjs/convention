using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public class Survey : IIndexableEntity
{
    public int Id { get; set; }
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key for Convention
    public int? ConventionId { get; set; }
    public virtual Convention? Convention { get; set; }

    public virtual ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
    public virtual ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
}