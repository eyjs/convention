namespace LocalRAG.Models
{
    public class SurveyResponseAnswer
    {
        public int Id { get; set; }
        public int SurveyResponseId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        public virtual SurveyResponse SurveyResponse { get; set; } = default!;
    }
}
