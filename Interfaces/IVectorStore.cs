namespace LocalRAG.Interfaces;

public interface IVectorStore
{
    Task<string> AddDocumentAsync(string content, float[] embedding, Dictionary<string, object>? metadata = null);
    Task<List<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<int> GetDocumentCountAsync();
}

public record VectorSearchResult(
    string DocumentId,
    string Content,
    float Similarity,
    Dictionary<string, object>? Metadata = null
);
