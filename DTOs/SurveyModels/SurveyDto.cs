using System.Collections.Generic;

namespace LocalRAG.DTOs.SurveyModels
{
    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int? ConventionId { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "SHORT_TEXT", "LONG_TEXT", "SINGLE_CHOICE", "MULTIPLE_CHOICE"
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }

    public class OptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
