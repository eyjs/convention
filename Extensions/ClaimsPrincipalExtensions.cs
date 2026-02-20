using System.Security.Claims;

namespace LocalRAG.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// 현재 사용자의 ID를 가져옵니다.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    /// <summary>
    /// 현재 사용자의 ID를 가져오거나, 클레임이 없으면 null을 반환합니다.
    /// </summary>
    public static int? GetUserIdOrNull(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
