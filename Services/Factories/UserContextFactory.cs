// Services/Factories/UserContextFactory.cs
using LocalRAG.Models;
using System.Security.Claims;

namespace LocalRAG.Services.Factories;

public class UserContextFactory : IUserContextFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextFactory> _logger;

    public UserContextFactory(IHttpContextAccessor httpContextAccessor, ILogger<UserContextFactory> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public ChatUserContext? CreateUserContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated. Creating an anonymous context.");
            return new ChatUserContext { Role = UserRole.Anonymous };
        }

        try
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            var guestIdClaim = user.FindFirst("GuestId")?.Value;

            if (!Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
            {
                userRole = UserRole.Anonymous;
                _logger.LogWarning("Could not parse user role from claims. Defaulting to Anonymous.");
            }

            int? guestId = int.TryParse(guestIdClaim, out var parsedGuestId) ? parsedGuestId : null;

            var context = new ChatUserContext
            {
                Role = userRole,
                GuestId = guestId,
                MemberId = user.FindFirst("LoginId")?.Value
            };

            _logger.LogInformation("Created user context from token: Role={Role}, GuestId={GuestId}", context.Role, context.GuestId);
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user context from claims.");
            return null; // 실패 시 null 반환
        }
    }
}