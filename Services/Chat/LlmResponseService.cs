using LocalRAG.DTOs.ChatModels;
using LocalRAG.Interfaces;
using LocalRAG.Services.Ai;

namespace LocalRAG.Services.Chat;

public class LlmResponseService
{
    private readonly LlmProviderManager _providerManager;
    public string ProviderName => _providerManager.GetActiveSettingAsync().GetAwaiter().GetResult()?.ProviderName ?? "default";

    public LlmResponseService(LlmProviderManager providerManager)
    {
        _providerManager = providerManager;
    }

    public async Task<string> GenerateResponseAsync(
        string question, string? context, List<ChatRequestMessage>? history, string? systemInstruction)
    {
        var provider = await _providerManager.GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(question, context, history, systemInstruction);
    }
}
