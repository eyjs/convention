namespace LocalRAG.Models
{
    public class Feature
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string IsEnabled { get; set; } = string.Empty;
    }
}