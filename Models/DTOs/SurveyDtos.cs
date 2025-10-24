using System.Collections.Generic;

namespace LocalRAG.Models.DTOs
{
    public class SurveyResponseDto
    {
        public List<SurveyResponseAnswerDto> Answers { get; set; } = new List<SurveyResponseAnswerDto>();
    }

    public class SurveyResponseAnswerDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}
