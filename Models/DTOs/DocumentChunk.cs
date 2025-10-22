using System.Collections.Generic;

namespace LocalRAG.Models.DTOs
{
    public class DocumentChunk
    {
        public required string Content { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}