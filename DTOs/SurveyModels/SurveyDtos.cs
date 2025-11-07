using System;
using System.Collections.Generic;

namespace LocalRAG.DTOs.SurveyModels
{
    public class SurveyResponseDto
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public int UserId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<SurveyResponseDetailDto> Answers { get; set; } = new List<SurveyResponseDetailDto>();
    }

    public class SurveyResponseDetailDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? AnswerText { get; set; }
    }
}