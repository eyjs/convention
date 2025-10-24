namespace LocalRAG.DTOs.AiModels
{
    public class AddDocumentRequest
    {
        public string Content { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}