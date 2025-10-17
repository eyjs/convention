using LocalRAG.Interfaces;
using LocalRAG.Models;

namespace LocalRAG.Services.ChatBot;

public class LlmResponseService
{
    private readonly ILlmProvider _provider;
    public string ProviderName => _provider.ProviderName;

    public LlmResponseService(ILlmProvider provider)
    {
        _provider = provider;
    }

    public async Task<string> GenerateResponseAsync(
        string question, string? context, List<ChatRequestMessage>? history, string? systemInstruction)
        => await _provider.GenerateResponseAsync(question, context, history, systemInstruction);
}
