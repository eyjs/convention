using LocalRAG.DTOs.ChatModels;

namespace LocalRAG.Interfaces;

public interface IChatHistoryService
{
    Task<bool> HasConventionAccessAsync(int userId, int conventionId, CancellationToken cancellationToken = default);
    Task<List<ChatHistoryMessageDto>> GetChatHistoryAsync(int conventionId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int userId, int conventionId, CancellationToken cancellationToken = default);
}
