namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    Task<string> GenerateResponseAsync(string prompt, string? context = null);
    Task<float[]> GenerateEmbeddingAsync(string text);
    string ProviderName { get; }
}
