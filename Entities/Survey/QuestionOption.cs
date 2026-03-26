using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    [Required]
    public string OptionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsTerminating { get; set; }

    public int? OptionTourId { get; set; }
    public virtual OptionTour? OptionTour { get; set; }

    public virtual SurveyQuestion Question { get; set; } = default!;
}