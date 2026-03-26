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
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ConventionId { get; set; }
        public string SurveyType { get; set; } = "GENERAL";
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
        /// <summary>
        /// 꼬리질문: 기존 옵션 ID (양수) 또는 프론트엔드 임시 키 (음수)
        /// null이면 최상위 질문
        /// </summary>
        public int? ParentOptionId { get; set; }
        public List<OptionCreateDto> Options { get; set; } = new List<OptionCreateDto>();
    }

    public class OptionCreateDto
    {
        public int? Id { get; set; }
        [Required]
        public string OptionText { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsTerminating { get; set; }
        public int? OptionTourId { get; set; }
        /// <summary>
        /// 프론트엔드가 새 옵션에 부여하는 임시 키 (음수). 꼬리질문 연결용.
        /// </summary>
        public int? TempKey { get; set; }
    }
}
