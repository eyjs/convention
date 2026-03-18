using System.Security.Claims;
using LocalRAG.DTOs.AuthModels;
using LocalRAG.Entities;

namespace LocalRAG.Interfaces;

public interface IAuthService
{
    // 기존 토큰/비밀번호 메서드
    string GenerateAccessToken(User user, int? guestId = null, int? conventionId = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);

    // 비즈니스 로직 메서드
    Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<AuthResult<GuestLoginResponse>> GuestLoginAsync(GuestLoginRequest request);
    Task<AuthResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(int userId);
    Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(int userId);
    Task<IEnumerable<AvailableConventionInfo>> GetAvailableConventionsAsync(int userId);
}

/// <summary>
/// 인증 서비스 결과 래퍼. HTTP 상태 코드 매핑을 위한 상태 정보를 포함합니다.
/// </summary>
public class AuthResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthErrorType ErrorType { get; set; }

    public static AuthResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static AuthResult<T> Fail(string message, AuthErrorType errorType = AuthErrorType.BadRequest) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = errorType
    };
}

public enum AuthErrorType
{
    BadRequest,
    Unauthorized,
    NotFound
}
