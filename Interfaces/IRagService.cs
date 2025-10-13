using LocalRAG.Interfaces;

namespace LocalRAG.Interfaces;

using LocalRAG.Models.DTOs;

public interface IRagService
{
    Task<string> AddDocumentAsync(string content, Dictionary<string, object>? metadata = null);
    Task AddDocumentsAsync(IEnumerable<DocumentChunk> chunks);
    Task<RagResponse> QueryAsync(string question, int topK = 5, Dictionary<string, object>? filter = null);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task DeleteDocumentsByMetadataAsync(string key, object value);
    Task<RagStats> GetStatsAsync();
}

public record RagResponse(
    string Answer,
    List<VectorSearchResult> Sources,
    string LlmProvider
);

public record RagStats(
    int DocumentCount,
    string EmbeddingModel,
    string LlmProvider
);
