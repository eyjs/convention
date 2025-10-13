using System.Collections.Generic;

namespace LocalRAG.Models.DTOs
{
    public class AddDocumentRequest
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
