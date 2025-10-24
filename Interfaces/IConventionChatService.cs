using LocalRAG.DTOs.ChatModels;

namespace LocalRAG.Interfaces;

public interface IConventionChatService
{
    Task<ChatResponse> AskAsync(
        string question,
        int? conventionId = null,
        ChatUserContext? userContext = null,
        List<ChatRequestMessage>? history = null);
    
    Task<List<string>> GetSuggestedQuestionsAsync(
        int conventionId,
        ChatUserContext? userContext);
}
