using LocalRAG.Models;

namespace LocalRAG.Interfaces;

public interface ILlmProvider
{
    string ProviderName { get; }

    Task<string> GenerateResponseAsync(string prompt, string? context = null, List<ChatRequestMessage>? history = null, string? systemInstructionOverride = null);

    Task<string> ClassifyIntentAsync(string question, List<ChatRequestMessage>? history = null);
}