using LocalRAG.DTOs.ChatModels;
using LocalRAG.Repositories;

namespace LocalRAG.Services.Chat;

public class ConventionAccessService
{
    private readonly IUnitOfWork _unitOfWork;

    public ConventionAccessService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> VerifyAsync(int conventionId, ChatUserContext userContext)
    {
        if (userContext.Role == UserRole.Admin) return true;
        if (userContext.UserId is null) return false;

        return await _unitOfWork.UserConventions.ExistsAsync(
            uc => uc.UserId == userContext.UserId && uc.ConventionId == conventionId);
    }
}
