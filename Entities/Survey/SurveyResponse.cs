using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public class SurveyResponse
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    [Required]
    public int UserId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public virtual Survey Survey { get; set; } = default!;
    public virtual User User { get; set; } = default!;
    public virtual ICollection<ResponseDetail> Details { get; set; } = new List<ResponseDetail>();
}