namespace LocalRAG.Models.DTOs
{
    public class AddDocumentResponse
    {
        public string DocumentId { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }

        public AddDocumentResponse(string documentId, bool success, string? error = null)
        {
            DocumentId = documentId;
            Success = success;
            Error = error;
        }
    }
}
