using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.SurveyModels
{
    public class SurveySubmissionDto
    {
        [Required]
        public int SurveyId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }

    public class AnswerDto
    {
        [Required]
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public List<int>? SelectedOptionIds { get; set; }
    }
}
