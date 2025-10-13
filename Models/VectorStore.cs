using System;

namespace LocalRAG.Models
{
    public class VectorStore
    {
        public Guid Id { get; set; }
        public int ConventionId { get; set; }
        public string SourceTable { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public DateTime RegDtm { get; set; }

        public Convention? Convention { get; set; }
    }
}
