using LocalRAG.DTOs.ChatModels;
using LocalRAG.Interfaces;
using System.Security.Claims;

namespace LocalRAG.Services.Shared;

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
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
            {
                userRole = UserRole.Anonymous;
                _logger.LogWarning("Could not parse user role from claims. Defaulting to Anonymous.");
            }

            int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;

            var context = new ChatUserContext
            {
                Role = userRole,
                UserId = userId,
                MemberId = user.FindFirst("LoginId")?.Value
            };

            _logger.LogInformation("Created user context from token: Role={Role}, UserId={UserId}", context.Role, context.UserId);
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user context from claims.");
            return null;
        }
    }
}
