using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LocalRAG.Controllers.Auth;

[ApiController]
[Route("api/auth")]
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
            // --- 1. 회원으로 로그인 시도 (ID: LoginId) ---

            // .Include(u => u.Guests)를 추가하여 사용자와 연결된 Guests 정보를 함께 로드합니다.
            var user = await _context.Users
                .Include(u => u.Guests)
                .FirstOrDefaultAsync(u => u.LoginId == request.LoginId);

            if (user != null && user.IsActive && _authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                var accessToken = _authService.GenerateAccessToken(user);
                var refreshToken = _authService.GenerateRefreshToken();
                
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    accessToken,
                    refreshToken,
                    user = new
                    {
                        id = user.Id,
                        loginId = user.LoginId,
                        name = user.Name,
                        role = user.Role
                    }
                });
            }

            // --- 2. 비회원으로 로그인 시도 (ID: 참석자 이름) ---
            // 이 부분은 비회원 로그인 로직이므로 기존 코드를 유지합니다.
            var nonMemberGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.GuestName == request.LoginId && !g.IsRegisteredUser);

            if (nonMemberGuest != null && !string.IsNullOrEmpty(nonMemberGuest.PasswordHash) && _authService.VerifyPassword(request.Password, nonMemberGuest.PasswordHash))
            {
                var guestUser = new User { Id = nonMemberGuest.Id, Name = nonMemberGuest.GuestName, Role = "Guest", LoginId = $"guest_{nonMemberGuest.Id}" };

                // (핵심 수정) 비회원 로그인 시에도 GuestId를 토큰에 포함시킵니다.
                var accessToken = _authService.GenerateAccessToken(guestUser, nonMemberGuest.Id, nonMemberGuest.ConventionId);
                var refreshToken = _authService.GenerateRefreshToken();

                return Ok(new
                {
                    accessToken,
                    refreshToken,
                    user = new
                    {
                        id = nonMemberGuest.Id,
                        guestId = nonMemberGuest.Id,
                        conventionId = nonMemberGuest.ConventionId,
                        name = nonMemberGuest.GuestName,
                        role = "Guest"
                    }
                });
            }

            // --- 3. 로그인 최종 실패 ---
            return Unauthorized(new { message = "아이디 또는 비밀번호를 확인해주세요." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {LoginId}", request.LoginId);
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

            var accessToken = _authService.GenerateAccessToken(guestUser, guest.Id, guest.ConventionId);

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

            var conventionList = new List<object>();
            foreach (var g in user.Guests.Where(g => g.Convention != null))
            {
                var unreadCount = await _context.ConventionChatMessages.CountAsync(m => 
                    m.ConventionId == g.ConventionId && 
                    m.CreatedAt > (g.LastChatReadTimestamp ?? DateTime.MinValue));

                conventionList.Add(new
                {
                    id = g.ConventionId,
                    title = g.Convention!.Title,
                    startDate = g.Convention.StartDate,
                    endDate = g.Convention.EndDate,
                    guestId = g.Id,
                    guestName = g.GuestName,
                    unreadCount = unreadCount
                });
            }

            // 현재 선택된 행사의 체크리스트 상태 계산 (selectedConventionId가 필요하면 쿠리 매개변수로 추가)
            // 여기서는 첨 번째 행사의 체크리스트를 기본으로 반환
            object? checklistStatus = null;
            var firstGuest = user.Guests.FirstOrDefault();
            
            _logger.LogInformation("GetCurrentUser: UserId={UserId}, GuestCount={GuestCount}, FirstGuest={FirstGuestId}", 
                user.Id, user.Guests.Count, firstGuest?.Id ?? 0);
            
            if (firstGuest != null)
            {
                checklistStatus = await BuildChecklistStatusAsync(firstGuest.Id, firstGuest.ConventionId);
                _logger.LogInformation("Checklist status built: {HasStatus}", checklistStatus != null);
            }

            return Ok(new
            {
                id = user.Id,
                loginId = user.LoginId,
                name = user.Name,
                email = user.Email,
                phone = user.Phone,
                role = user.Role,
                profileImageUrl = user.ProfileImageUrl,
                conventions = conventionList,
                checklistStatus = checklistStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user error");
            return StatusCode(500, new { message = "사용자 정보 조회 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 체크리스트 상태 구축
    /// </summary>
    private async Task<object?> BuildChecklistStatusAsync(int guestId, int conventionId)
    {
        _logger.LogInformation("BuildChecklistStatusAsync called for GuestId={GuestId}, ConventionId={ConventionId}", guestId, conventionId);
        
        // 1. 해당 행사의 활성 액션 목록 조회
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId && a.IsActive)
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Found {ActionCount} active actions for ConventionId={ConventionId}", actions.Count, conventionId);

        if (actions.Count == 0)
            return null; // 액션이 없으면 null 반환 (국내 행사 등)

        // 2. 해당 참여자의 액션 상태 조회
        var statuses = await _context.GuestActionStatuses
            .Where(s => s.GuestId == guestId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

        // 3. 체크리스트 아이템 구축
        var items = new List<object>();
        int completedCount = 0;

        foreach (var action in actions)
        {
            var status = statusDict.GetValueOrDefault(action.Id);
            bool isComplete = status?.IsComplete ?? false;

            if (isComplete)
                completedCount++;

            items.Add(new
            {
                actionId = action.Id,
                actionType = action.ActionType,
                title = action.Title,
                isComplete = isComplete,
                deadline = action.Deadline,
                navigateTo = action.MapsTo,
                orderNum = action.OrderNum
            });
        }

        // 4. 가장 가까운 미완료 액션의 마감일 찾기
        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false) && a.Deadline.HasValue;
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        // 5. 체크리스트 DTO 반환
        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? (completedCount * 100 / totalItems) : 0;

        return new
        {
            totalItems = totalItems,
            completedItems = completedCount,
            progressPercentage = progressPercentage,
            overallDeadline = overallDeadline,
            items = items
        };
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
