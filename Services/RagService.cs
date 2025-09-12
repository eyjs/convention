using LocalRAG.Interfaces;

namespace LocalRAG.Services;

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

    public async Task<RagResponse> QueryAsync(string question, int topK = 5)
    {
        // 1. 질문을 벡터로 변환
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
        
        // 2. 유사한 문서 검색
        var searchResults = await _vectorStore.SearchAsync(queryEmbedding, topK);
        
        // 3. 컨텍스트 구성
        var context = string.Join("\n\n", searchResults.Select((r, i) => $"[{i + 1}] {r.Content}"));
        
        // 4. LLM으로 답변 생성
        var answer = await _llmProvider.GenerateResponseAsync(question, context);
        
        return new RagResponse(answer, searchResults, _llmProvider.ProviderName);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        return await _vectorStore.DeleteDocumentAsync(documentId);
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
