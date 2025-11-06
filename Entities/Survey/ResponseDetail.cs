namespace LocalRAG.Entities;

public class ResponseDetail
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    
    public string? AnswerText { get; set; }
    public int? SelectedOptionId { get; set; }

    public virtual SurveyResponse Response { get; set; } = default!;
    public virtual SurveyQuestion Question { get; set; } = default!;
    public virtual QuestionOption? SelectedOption { get; set; }
}