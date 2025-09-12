using LocalRAG.Interfaces;
using System.Collections.Concurrent;

namespace LocalRAG.Storage;

public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, VectorDocument> _documents = new();
    
    private record VectorDocument(
        string Id,
        string Content,
        float[] Embedding,
        Dictionary<string, object>? Metadata,
        DateTime CreatedAt
    );

    public Task<string> AddDocumentAsync(string content, float[] embedding, Dictionary<string, object>? metadata = null)
    {
        var documentId = Guid.NewGuid().ToString();
        var document = new VectorDocument(
            documentId,
            content,
            embedding,
            metadata,
            DateTime.UtcNow
        );
        
        _documents[documentId] = document;
        return Task.FromResult(documentId);
    }

    public Task<List<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5)
    {
        var results = _documents.Values
            .Select(doc => new VectorSearchResult(
                doc.Id,
                doc.Content,
                CalculateCosineSimilarity(queryEmbedding, doc.Embedding),
                doc.Metadata
            ))
            .OrderByDescending(r => r.Similarity)
            .Take(topK)
            .ToList();
            
        return Task.FromResult(results);
    }

    public Task<bool> DeleteDocumentAsync(string documentId)
    {
        return Task.FromResult(_documents.TryRemove(documentId, out _));
    }

    public Task<int> GetDocumentCountAsync()
    {
        return Task.FromResult(_documents.Count);
    }

    private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vector dimensions must match");

        var dotProduct = 0.0f;
        var magnitudeA = 0.0f;
        var magnitudeB = 0.0f;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = (float)Math.Sqrt(magnitudeA);
        magnitudeB = (float)Math.Sqrt(magnitudeB);

        if (magnitudeA == 0.0f || magnitudeB == 0.0f)
            return 0.0f;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
