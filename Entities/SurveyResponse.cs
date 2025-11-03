using LocalRAG.Entities.Action;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities
{
    /// <summary>
    /// 설문 응답
    /// </summary>
    public class SurveyResponse
    {
        public int Id { get; set; }
        public int ConventionActionId { get; set; }

        /// <summary>
        /// 응답자 User ID (FK)
        /// </summary>
        public int UserId { get; set; }

        public DateTime SubmittedAt { get; set; }

        [ForeignKey("ConventionActionId")]
        public virtual ConventionAction ConventionAction { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;

        public virtual ICollection<SurveyResponseAnswer> Answers { get; set; } = new List<SurveyResponseAnswer>();
    }
}
