using LocalRAG.Interfaces;
using LocalRAG.DTOs;
using LocalRAG.DTOs.AiModels;

namespace LocalRAG.Services.Ai;

public class RagService : IRagService
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmProvider _llmProvider;

    public RagService(
        IVectorStore vectorStore,
        IEmbeddingService embeddingService,
        ILlmProvider llmProvider)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _llmProvider = llmProvider;
    }
    
    public async Task<string> AddDocumentAsync(string content, Dictionary<string, object>? metadata = null)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(content);
        return await _vectorStore.AddDocumentAsync(content, embedding, metadata);
    }

    public async Task AddDocumentsAsync(IEnumerable<DocumentChunk> chunks)
    {
        var vectorDocuments = new List<VectorDocument>();
        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
            vectorDocuments.Add(new VectorDocument(chunk.Content, embedding, chunk.Metadata));
        }
        await _vectorStore.AddDocumentsAsync(vectorDocuments);
    }

    public async Task<RagResponse> QueryAsync(string question, int topK = 5, Dictionary<string, object>? filter = null)
    {
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
        var searchResults = await _vectorStore.SearchAsync(queryEmbedding, topK, filter);
        var context = string.Join("\n\n", searchResults.Select((r, i) => $"[{i + 1}] {r.Content}"));
        var answer = await _llmProvider.GenerateResponseAsync(question, context);
        
        return new RagResponse(answer, searchResults, _llmProvider.ProviderName);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        return await _vectorStore.DeleteDocumentAsync(documentId);
    }

    public async Task DeleteDocumentsByMetadataAsync(string key, object value)
    {
        await _vectorStore.DeleteDocumentsByMetadataAsync(key, value);
    }

    public async Task<RagStats> GetStatsAsync()
    {
        var documentCount = await _vectorStore.GetDocumentCountAsync();
        return new RagStats(
            documentCount,
            "LocalEmbedding", // TODO: 실제 모델명으로 변경
            _llmProvider.ProviderName
        );
    }
}
