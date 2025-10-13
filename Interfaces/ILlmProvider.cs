using LocalRAG.Models;

namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    Task<string> GenerateResponseAsync(string prompt, string? context = null, List<ChatMessage>? history = null);

    Task<string> ClassifyIntentAsync(string question, List<ChatMessage>? history = null);
}