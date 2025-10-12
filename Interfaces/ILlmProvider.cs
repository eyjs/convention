namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    // [수정] 사용자 컨텍스트(userContext)를 전달받는 파라미터를 추가합니다.
    Task<string> GenerateResponseAsync(string prompt, string? context = null, string? userContext = null);

    Task<float[]> GenerateEmbeddingAsync(string text);
}