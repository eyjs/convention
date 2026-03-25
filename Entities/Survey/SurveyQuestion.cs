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

    // 꼬리질문: 이 옵션을 선택하면 이 질문이 표시됨 (null이면 최상위 질문)
    public int? ParentOptionId { get; set; }
    public virtual QuestionOption? ParentOption { get; set; }

    public virtual Survey Survey { get; set; } = default!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}