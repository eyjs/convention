using LocalRAG.Interfaces;

namespace LocalRAG.Interfaces;

public interface IRagService
{
    Task<string> AddDocumentAsync(string content, Dictionary<string, object>? metadata = null);
    Task<RagResponse> QueryAsync(string question, int topK = 5);
    Task<bool> DeleteDocumentAsync(string documentId);
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
