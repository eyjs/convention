namespace LocalRAG.Interfaces;

public record VectorDocument(
    string Content,
    float[] Embedding,
    Dictionary<string, object>? Metadata = null
);

public interface IVectorStore
{
    Task<string> AddDocumentAsync(string content, float[] embedding, Dictionary<string, object>? metadata = null);
    Task AddDocumentsAsync(IEnumerable<VectorDocument> documents);
    Task<List<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, Dictionary<string, object>? filter = null);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task DeleteDocumentsByMetadataAsync(string key, object value);
    Task DeleteDocumentsByConventionIdAsync(int conventionId);
    Task<int> GetDocumentCountAsync();
    Task<int> GetDocumentCountAsync(int conventionId);
}

public record VectorSearchResult(
    string DocumentId,
    string Content,
    float Similarity,
    Dictionary<string, object>? Metadata = null
);
