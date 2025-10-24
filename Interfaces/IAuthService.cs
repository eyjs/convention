using System.Security.Claims;
using LocalRAG.Entities;

namespace LocalRAG.Interfaces;

public interface IAuthService
{
    string GenerateAccessToken(User user, int? guestId = null, int? conventionId = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
