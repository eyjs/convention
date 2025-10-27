using LocalRAG.DTOs.ChatModels;
using LocalRAG.Interfaces;
using LocalRAG.Services.Ai;

namespace LocalRAG.Services.Chat;

public class SourceIdentifier
{
    private readonly ChatIntentRouter _chatIntentRouter;
    private readonly ILogger<SourceIdentifier> _logger;

    public enum SourceType
    {
        Personal_DB,      // 개인 정보 (DB)
        Schedule_DB,      // 일정 정보 (DB)
        RAG_Convention,   // 행사 RAG (Vector DB)
        RAG_Global,       // 전역 RAG (Vector DB)
        General_LLM       // 일반 LLM 지식
    }

    public SourceIdentifier(ChatIntentRouter chatIntentRouter, ILogger<SourceIdentifier> logger)
    {
        _chatIntentRouter = chatIntentRouter;
        _logger = logger;
    }

    public async Task<List<SourceType>> GetRequiredSourcesAsync(string question, List<ChatRequestMessage>? history)
    {
        var intent = await _chatIntentRouter.GetIntentAsync(question, history);
        var sources = new List<SourceType>();

        switch (intent)
        {
            case ChatIntentRouter.Intent.PersonalInfo:
                sources.Add(SourceType.Personal_DB);
                break;
            case ChatIntentRouter.Intent.PersonalSchedule:
                sources.Add(SourceType.Schedule_DB);
                break;
            case ChatIntentRouter.Intent.Event:
                sources.Add(SourceType.RAG_Convention);
                sources.Add(SourceType.RAG_Global);
                break;
            case ChatIntentRouter.Intent.General:
                sources.Add(SourceType.General_LLM);
                break;
            case ChatIntentRouter.Intent.Unknown:
                // Unknown intent might still benefit from RAG or general LLM
                sources.Add(SourceType.RAG_Convention);
                sources.Add(SourceType.RAG_Global);
                sources.Add(SourceType.General_LLM);
                break;
        }

        if (sources.Count == 0) // Fallback if no specific source is identified
        {
            _logger.LogWarning("No specific sources identified for intent {Intent}. Defaulting to General_LLM.", intent);
            sources.Add(SourceType.General_LLM);
        }

        return sources;
    }
}
