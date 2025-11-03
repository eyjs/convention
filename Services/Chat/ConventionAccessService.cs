using LocalRAG.Data;
using LocalRAG.DTOs.ChatModels;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Chat;

public class ConventionAccessService
{
    private readonly ConventionDbContext _context;

    public ConventionAccessService(ConventionDbContext context)
    {
        _context = context;
    }

    public async Task<bool> VerifyAsync(int conventionId, ChatUserContext userContext)
    {
        if (userContext.Role == UserRole.Admin) return true;
        if (userContext.UserId is null) return false;

        return await _context.UserConventions
            .AnyAsync(uc => uc.UserId == userContext.UserId && uc.ConventionId == conventionId);
    }
}
