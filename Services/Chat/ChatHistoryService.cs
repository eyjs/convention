using LocalRAG.DTOs.ChatModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Chat;

public class ChatHistoryService : IChatHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasConventionAccessAsync(int userId, int conventionId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.UserConventions
            .ExistsAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId, cancellationToken);
    }

    public async Task<List<ChatHistoryMessageDto>> GetChatHistoryAsync(int conventionId, CancellationToken cancellationToken = default)
    {
        var messages = await _unitOfWork.ConventionChatMessages.Query
            .Where(m => m.ConventionId == conventionId)
            .OrderBy(m => m.CreatedAt)
            .Join(
                _unitOfWork.Users.Query,
                chatMessage => chatMessage.UserId,
                user => user.Id,
                (chatMessage, user) => new ChatHistoryMessageDto
                {
                    userId = chatMessage.UserId,
                    userName = chatMessage.IsAdmin
                        ? $"{user.Name}"
                        : user.Name,
                    profileImageUrl = user.ProfileImageUrl,
                    message = chatMessage.Message,
                    createdAt = chatMessage.CreatedAt.ToString("o"),
                    isAdmin = chatMessage.IsAdmin
                })
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task MarkAsReadAsync(int userId, int conventionId, CancellationToken cancellationToken = default)
    {
        var userConvention = await _unitOfWork.UserConventions
            .GetByUserAndConventionAsync(userId, conventionId, cancellationToken);

        if (userConvention == null)
            return;

        userConvention.LastChatReadTimestamp = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
