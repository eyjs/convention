using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    [Required]
    public string OptionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    public virtual SurveyQuestion Question { get; set; } = default!;
}