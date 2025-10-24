namespace LocalRAG.Models
{
    public class SurveyResponse
    {
        public int Id { get; set; }
        public int ConventionActionId { get; set; }
        public int GuestId { get; set; }
        public DateTime SubmittedAt { get; set; }

        public virtual ConventionAction ConventionAction { get; set; } = default!;
        public virtual Guest Guest { get; set; } = default!;
        public virtual ICollection<SurveyResponseAnswer> Answers { get; set; } = new List<SurveyResponseAnswer>();
    }
}
