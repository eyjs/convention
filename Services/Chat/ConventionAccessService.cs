using LocalRAG.Data;
using LocalRAG.Models;
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
        if (userContext.GuestId is null) return false;

        return await _context.Guests
            .AnyAsync(g => g.Id == userContext.GuestId && g.ConventionId == conventionId);
    }
}
