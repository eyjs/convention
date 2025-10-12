namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    Task<string> GenerateResponseAsync(string prompt, string? context = null, string? userContext = null);

    Task<string> ClassifyIntentAsync(string question);

    Task<float[]> GenerateEmbeddingAsync(string text);
}