namespace LocalRAG.DTOs.AuthModels;

/// <summary>
/// 회원가입 응답
/// </summary>
public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 로그인 응답
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public LoginUserInfo User { get; set; } = null!;
}

public class LoginUserInfo
{
    public int Id { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// 비회원 로그인 응답
/// </summary>
public class GuestLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public bool IsGuest { get; set; } = true;
    public GuestUserInfo User { get; set; } = null!;
    public GuestConventionInfo Convention { get; set; } = null!;
}

public class GuestUserInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CorpPart { get; set; }
    public string? Affiliation { get; set; }
}

public class GuestConventionInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 회원 전환 안내 응답 (비회원 로그인 시 회원 전환된 경우)
/// </summary>
public class GuestLoginConvertedResponse
{
    public string Message { get; set; } = string.Empty;
    public string LoginId { get; set; } = string.Empty;
}

/// <summary>
/// 토큰 갱신 응답
/// </summary>
public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 현재 사용자 정보 응답
/// </summary>
public class CurrentUserResponse
{
    public int Id { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public List<UserConventionInfo> Conventions { get; set; } = new();
    public object? ChecklistStatus { get; set; }
}

public class UserConventionInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
}

/// <summary>
/// 참여 가능한 행사 정보
/// </summary>
public class AvailableConventionInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ConventionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ConventionImg { get; set; }
}
