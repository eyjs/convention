using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.SurveyModels
{
    public class SurveyCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int? ConventionId { get; set; }
        public List<QuestionCreateDto> Questions { get; set; } = new List<QuestionCreateDto>();
    }

    public class QuestionCreateDto
    {
        public int? Id { get; set; }
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty; // "SHORT_TEXT", "LONG_TEXT", "SINGLE_CHOICE", "MULTIPLE_CHOICE"
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public List<OptionCreateDto> Options { get; set; } = new List<OptionCreateDto>();
    }

    public class OptionCreateDto
    {
        public int? Id { get; set; }
        [Required]
        public string OptionText { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
