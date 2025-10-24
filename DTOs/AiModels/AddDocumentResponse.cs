namespace LocalRAG.DTOs.AiModels
{
    public record AddDocumentResponse(string DocumentId, bool Success, string? ErrorMessage = null);
}