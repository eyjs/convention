using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ConventionDbContext context, 
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.LoginId == request.LoginId);

            if (existingUser != null)
            {
                return BadRequest(new { message = "이미 사용 중인 아이디입니다." });
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = "이미 사용 중인 이메일입니다." });
                }
            }

            var user = new User
            {
                LoginId = request.LoginId,
                PasswordHash = _authService.HashPassword(request.Password),
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Role = "Guest",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {LoginId}", user.LoginId);

            return Ok(new
            {
                message = "회원가입이 완료되었습니다.",
                userId = user.Id,
                loginId = user.LoginId,
                name = user.Name
            });
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
            var user = await _context.Users
                .Include(u => u.Guests)
                    .ThenInclude(g => g.Convention)
                .FirstOrDefaultAsync(u => u.LoginId == request.LoginId);

            if (user == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호를 확인해주세요." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "비활성화된 계정입니다." });
            }

            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호를 확인해주세요." });
            }

            var accessToken = _authService.GenerateAccessToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var conventions = user.Guests
                .Where(g => g.Convention != null)
                .Select(g => new
                {
                    id = g.ConventionId,
                    title = g.Convention!.Title,
                    startDate = g.Convention.StartDate,
                    endDate = g.Convention.EndDate
                })
                .Distinct()
                .ToList();

            return Ok(new
            {
                accessToken,
                refreshToken,
                user = new
                {
                    id = user.Id,
                    loginId = user.LoginId,
                    name = user.Name,
                    email = user.Email,
                    phone = user.Phone,
                    role = user.Role,
                    profileImageUrl = user.ProfileImageUrl
                },
                conventions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, new { message = "로그인 중 오류가 발생했습니다." });
        }
    }

    // 비회원 로그인 (이름 + 연락처)
    [HttpPost("guest-login")]
    public async Task<IActionResult> GuestLogin([FromBody] GuestLoginRequest request)
    {
        try
        {
            // 연락처에서 '-' 제거
            var normalizedPhone = request.Phone.Replace("-", "").Replace(" ", "");

            var guest = await _context.Guests
                .Include(g => g.Convention)
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => 
                    g.GuestName == request.Name && 
                    g.Telephone.Replace("-", "").Replace(" ", "") == normalizedPhone &&
                    g.ConventionId == request.ConventionId);

            if (guest == null)
            {
                return Unauthorized(new { message = "참석자 정보를 찾을 수 없습니다. 이름과 연락처를 확인해주세요." });
            }

            // 회원으로 전환된 경우
            if (guest.IsRegisteredUser && guest.User != null)
            {
                return BadRequest(new { 
                    message = "회원으로 전환된 계정입니다. 일반 로그인을 이용해주세요.",
                    loginId = guest.User.LoginId
                });
            }

            // 비회원용 토큰 생성 (Guest 정보 기반)
            var guestUser = new User
            {
                Id = guest.Id,
                LoginId = $"guest_{guest.Id}",
                Name = guest.GuestName,
                Phone = guest.Telephone,
                Role = "Guest"
            };

            var accessToken = _authService.GenerateAccessToken(guestUser);

            return Ok(new
            {
                accessToken,
                isGuest = true,
                guest = new
                {
                    id = guest.Id,
                    name = guest.GuestName,
                    phone = guest.Telephone,
                    corpPart = guest.CorpPart,
                    affiliation = guest.Affiliation
                },
                convention = new
                {
                    id = guest.Convention.Id,
                    title = guest.Convention.Title,
                    startDate = guest.Convention.StartDate,
                    endDate = guest.Convention.EndDate
                }
            });
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "유효하지 않은 리프레시 토큰입니다." });
            }

            var accessToken = _authService.GenerateAccessToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken
            });
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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                await _context.SaveChangesAsync();
            }

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
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.Guests)
                    .ThenInclude(g => g.Convention)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "사용자를 찾을 수 없습니다." });
            }

            var conventions = user.Guests
                .Where(g => g.Convention != null)
                .Select(g => new
                {
                    id = g.ConventionId,
                    title = g.Convention!.Title,
                    startDate = g.Convention.StartDate,
                    endDate = g.Convention.EndDate,
                    guestId = g.Id,
                    guestName = g.GuestName
                })
                .ToList();

            return Ok(new
            {
                id = user.Id,
                loginId = user.LoginId,
                name = user.Name,
                email = user.Email,
                phone = user.Phone,
                role = user.Role,
                profileImageUrl = user.ProfileImageUrl,
                conventions
            });
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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var joinedConventionIds = await _context.Guests
                .Where(g => g.UserId == userId)
                .Select(g => g.ConventionId)
                .ToListAsync();

            var availableConventions = await _context.Conventions
                .Where(c => c.DeleteYn == "N" && !joinedConventionIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.ConventionType,
                    c.StartDate,
                    c.EndDate,
                    c.ConventionImg
                })
                .ToListAsync();

            return Ok(availableConventions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get available conventions error");
            return StatusCode(500, new { message = "행사 목록 조회 중 오류가 발생했습니다." });
        }
    }
}

public class RegisterRequest
{
    public string LoginId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class LoginRequest
{
    public string LoginId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GuestLoginRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int ConventionId { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
