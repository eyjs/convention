using LocalRAG.Extensions;
using LocalRAG.Interfaces;
using LocalRAG.DTOs.AuthModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return result.IsSuccess
                ? Ok(result.Data)
                : ToErrorResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(500, new { message = "회원가입 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return result.IsSuccess
                ? Ok(result.Data)
                : ToErrorResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {LoginId}", request.LoginId);
            return StatusCode(500, new { message = "로그인 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// AccessToken 기반 자동 로그인 (알림톡 딥링크용)
    /// </summary>
    [HttpPost("token-login")]
    public async Task<IActionResult> TokenLogin([FromBody] TokenLoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
                return BadRequest(new { message = "토큰이 없습니다." });

            var result = await _authService.TokenLoginAsync(request.AccessToken);
            if (!result.IsSuccess) return Unauthorized(new { message = result.ErrorMessage });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token login error");
            return StatusCode(500, new { message = "로그인 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("guest-login")]
    public async Task<IActionResult> GuestLogin([FromBody] GuestLoginRequest request)
    {
        try
        {
            var result = await _authService.GuestLoginAsync(request);

            if (!result.IsSuccess)
            {
                // 회원 전환 안내 (메시지에 '|' 구분자로 loginId가 포함된 경우)
                if (result.ErrorMessage != null && result.ErrorMessage.Contains('|'))
                {
                    var parts = result.ErrorMessage.Split('|', 2);
                    return BadRequest(new { message = parts[0], loginId = parts[1] });
                }

                return ToErrorResponse(result);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Guest login error");
            return StatusCode(500, new { message = "로그인 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request);
            return result.IsSuccess
                ? Ok(result.Data)
                : ToErrorResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh error");
            return StatusCode(500, new { message = "토큰 갱신 중 오류가 발생했습니다." });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.GetUserId();
            await _authService.LogoutAsync(userId);
            return Ok(new { message = "로그아웃되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, new { message = "로그아웃 중 오류가 발생했습니다." });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        _logger.LogInformation("GetCurrentUser endpoint hit.");
        try
        {
            var userId = User.GetUserId();
            var result = await _authService.GetCurrentUserAsync(userId);
            return result.IsSuccess
                ? Ok(result.Data)
                : ToErrorResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error");
            return StatusCode(500, new { message = "사용자 정보 조회 중 오류가 발생했습니다." });
        }
    }

    [Authorize]
    [HttpGet("conventions/available")]
    public async Task<IActionResult> GetAvailableConventions()
    {
        try
        {
            var userId = User.GetUserId();
            var conventions = await _authService.GetAvailableConventionsAsync(userId);
            return Ok(conventions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get available conventions error");
            return StatusCode(500, new { message = "행사 목록 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// AuthResult의 ErrorType에 따라 적절한 HTTP 응답을 반환합니다.
    /// </summary>
    private IActionResult ToErrorResponse<T>(AuthResult<T> result)
    {
        return result.ErrorType switch
        {
            AuthErrorType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
            AuthErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }
}
