using System.Collections.Generic;

namespace LocalRAG.DTOs.SurveyModels
{
    public class SurveyStatsDto
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public List<QuestionStatsDto> QuestionStats { get; set; } = new List<QuestionStatsDto>();
    }

    public class QuestionStatsDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public List<AnswerStatDto> Answers { get; set; } = new List<AnswerStatDto>();
    }

    public class AnswerStatDto
    {
        public string Answer { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
