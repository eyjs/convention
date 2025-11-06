using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Entities;

public enum QuestionType
{
    SHORT_TEXT,
    LONG_TEXT,
    SINGLE_CHOICE,
    MULTIPLE_CHOICE
}

public class SurveyQuestion
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; } = false;
    public int OrderIndex { get; set; }

    public virtual Survey Survey { get; set; } = default!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}