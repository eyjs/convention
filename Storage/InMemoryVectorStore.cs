using LocalRAG.Interfaces;
using System.Collections.Concurrent;

namespace LocalRAG.Storage;

public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, (VectorDocument doc, DateTime createdAt)> _documents = new();

    public Task<string> AddDocumentAsync(string content, float[] embedding, Dictionary<string, object>? metadata = null)
    {
        var documentId = Guid.NewGuid().ToString();
        var document = new VectorDocument(content, embedding, metadata);
        
        _documents[documentId] = (document, DateTime.UtcNow);
        return Task.FromResult(documentId);
    }

    public Task AddDocumentsAsync(IEnumerable<VectorDocument> documents)
    {
        foreach (var doc in documents)
        {
            var documentId = Guid.NewGuid().ToString();
            _documents[documentId] = (doc, DateTime.UtcNow);
        }
        return Task.CompletedTask;
    }

    public Task<List<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, Dictionary<string, object>? filter = null)
    {
        IEnumerable<KeyValuePair<string, (VectorDocument doc, DateTime createdAt)>> filteredDocuments = _documents;

        if (filter != null && filter.Any())
        {
            filteredDocuments = filteredDocuments.Where(kvp =>
            {
                var doc = kvp.Value.doc;
                if (doc.Metadata == null) return false;

                foreach (var condition in filter)
                {
                    if (!doc.Metadata.TryGetValue(condition.Key, out var metadataValue))
                    {
                        return false;
                    }

                    if (condition.Value is not null && metadataValue is not null)
                    {
                        // 숫자 형식 변환 및 비교
                        bool isConditionNumeric = long.TryParse(condition.Value.ToString(), out long conditionAsLong);
                        bool isMetadataNumeric = long.TryParse(metadataValue.ToString(), out long metadataAsLong);

                        if (isConditionNumeric && isMetadataNumeric)
                        {
                            if (conditionAsLong != metadataAsLong)
                            {
                                return false;
                            }
                        }
                        else // 문자열 비교
                        {
                            if (!string.Equals(condition.Value.ToString(), metadataValue.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                    else if (condition.Value != metadataValue)
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        var results = filteredDocuments
            .Select(kvp => new VectorSearchResult(
                kvp.Key,
                kvp.Value.doc.Content,
                CalculateCosineSimilarity(queryEmbedding, kvp.Value.doc.Embedding),
                kvp.Value.doc.Metadata
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

    public Task DeleteDocumentsByMetadataAsync(string key, object value)
    {
        var keysToRemove = _documents.Where(kvp =>
            kvp.Value.doc.Metadata != null &&
            kvp.Value.doc.Metadata.TryGetValue(key, out var metadataValue) &&
            (value == null ? metadataValue == null : value.Equals(metadataValue)))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var k in keysToRemove)
        {
            _documents.TryRemove(k, out _);
        }

        return Task.CompletedTask;
    }

    public Task<int> GetDocumentCountAsync()
    {
        return Task.FromResult(_documents.Count);
    }

    public Task<int> GetDocumentCountAsync(int conventionId)
    {
        var count = _documents.Values.Count(d =>
            d.doc.Metadata != null &&
            d.doc.Metadata.TryGetValue("convention_id", out var id) &&
            id is int && (int)id == conventionId);
        return Task.FromResult(count);
    }

    public Task DeleteDocumentsByConventionIdAsync(int conventionId)
    {
        var keysToRemove = _documents.Where(kvp =>
            kvp.Value.doc.Metadata != null &&
            kvp.Value.doc.Metadata.TryGetValue("convention_id", out var idObj) &&
            idObj is int id && id == conventionId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var k in keysToRemove)
        {
            _documents.TryRemove(k, out _);
        }

        return Task.CompletedTask;
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
