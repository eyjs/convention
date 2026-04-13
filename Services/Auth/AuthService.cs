using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using LocalRAG.Configuration;
using LocalRAG.Constants;
using LocalRAG.DTOs.AuthModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using User = LocalRAG.Entities.User;

namespace LocalRAG.Services.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChecklistService _checklistService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        JwtSettings jwtSettings,
        IUnitOfWork unitOfWork,
        IChecklistService checklistService,
        ILogger<AuthService> logger)
    {
        _jwtSettings = jwtSettings;
        _unitOfWork = unitOfWork;
        _checklistService = checklistService;
        _logger = logger;
    }

    // ============================================================
    // 토큰/비밀번호 메서드 (기존)
    // ============================================================

    public string GenerateAccessToken(User user, int? guestId = null, int? conventionId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("LoginId", user.LoginId)
        };

        if (guestId.HasValue)
        {
            claims.Add(new Claim("GuestId", guestId.Value.ToString()));
        }

        if (conventionId.HasValue)
        {
            claims.Add(new Claim("ConventionId", conventionId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }

    // ============================================================
    // 비즈니스 로직 메서드
    // ============================================================

    public async Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        // LoginId 공백 제거 — 로그인 시 불일치 방지
        var loginId = request.LoginId?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(loginId))
        {
            return AuthResult<RegisterResponse>.Fail("아이디를 입력해주세요.");
        }

        var existingUser = await _unitOfWork.Users.GetByLoginIdAsync(loginId);
        if (existingUser != null)
        {
            return AuthResult<RegisterResponse>.Fail("이미 사용 중인 아이디입니다.");
        }

        var email = request.Email?.Trim();
        if (!string.IsNullOrEmpty(email))
        {
            var existingEmail = await _unitOfWork.Users.GetAsync(u => u.Email == email);
            if (existingEmail != null)
            {
                return AuthResult<RegisterResponse>.Fail("이미 사용 중인 이메일입니다.");
            }
        }

        var user = new User
        {
            LoginId = loginId,
            PasswordHash = HashPassword(request.Password),
            Name = request.Name?.Trim() ?? string.Empty,
            Email = email,
            Phone = request.Phone?.Trim(),
            Role = Roles.User, // 일반 회원가입은 User 역할
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("New user registered: LoginId={LoginId}, Id={Id}, HashLen={HashLen}",
            user.LoginId, user.Id, user.PasswordHash?.Length ?? 0);

        return AuthResult<RegisterResponse>.Success(new RegisterResponse
        {
            Message = "회원가입이 완료되었습니다.",
            UserId = user.Id,
            LoginId = user.LoginId,
            Name = user.Name
        });
    }

    public async Task<AuthResult<LoginResponse>> TokenLoginAsync(string accessToken)
    {
        var uc = await _unitOfWork.UserConventions.GetByAccessTokenAsync(accessToken);
        if (uc == null)
            return AuthResult<LoginResponse>.Fail("유효하지 않은 토큰입니다.", AuthErrorType.Unauthorized);

        var user = await _unitOfWork.Users.GetByIdAsync(uc.UserId);
        if (user == null)
            return AuthResult<LoginResponse>.Fail("사용자를 찾을 수 없습니다.", AuthErrorType.Unauthorized);

        var jwtToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(14);
        await _unitOfWork.SaveChangesAsync();

        return AuthResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = jwtToken,
            RefreshToken = refreshToken,
            User = new LoginUserInfo
            {
                Id = user.Id,
                LoginId = user.LoginId,
                Name = user.Name ?? string.Empty,
                Role = user.Role
            }
        });
    }

    public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var loginId = request.LoginId?.Trim() ?? string.Empty;
        // tracking: true — 아래에서 바로 수정할 것이므로 tracked 엔티티로 조회
        var user = await _unitOfWork.Users.GetByLoginIdAsync(loginId, tracking: true);

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for LoginId={LoginId}", loginId);
            return AuthResult<LoginResponse>.Fail("아이디 또는 비밀번호를 확인해주세요.", AuthErrorType.Unauthorized);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: user {LoginId} is inactive", loginId);
            return AuthResult<LoginResponse>.Fail("아이디 또는 비밀번호를 확인해주세요.", AuthErrorType.Unauthorized);
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: password mismatch for {LoginId}, hash length={HashLength}", loginId, user.PasswordHash?.Length ?? 0);
            return AuthResult<LoginResponse>.Fail("아이디 또는 비밀번호를 확인해주세요.", AuthErrorType.Unauthorized);
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return AuthResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new LoginUserInfo
            {
                Id = user.Id,
                LoginId = user.LoginId,
                Name = user.Name,
                Role = user.Role
            }
        });
    }

    public async Task<AuthResult<GuestLoginResponse>> GuestLoginAsync(GuestLoginRequest request)
    {
        var normalizedPhone = request.Phone.Replace("-", "").Replace(" ", "");

        var userConvention = await _unitOfWork.UserConventions
            .GetUserConventionWithUserAsync(request.ConventionId, request.Name, normalizedPhone);

        if (userConvention == null)
        {
            return AuthResult<GuestLoginResponse>.Fail(
                "참석자 정보를 찾을 수 없습니다. 이름과 연락처를 확인해주세요.",
                AuthErrorType.Unauthorized);
        }

        if (userConvention.User == null)
        {
            _logger.LogWarning("Guest login failed due to partial data. UserId: {UserId}, ConventionId: {ConventionId}", userConvention.UserId, userConvention.ConventionId);
            return AuthResult<GuestLoginResponse>.Fail(
                "참석자 또는 행사 정보를 찾을 수 없습니다.",
                AuthErrorType.Unauthorized);
        }

        var user = userConvention.User;

        // 회원으로 전환된 경우
        if (!string.IsNullOrEmpty(user.LoginId) && !user.LoginId.StartsWith("guest_"))
        {
            return AuthResult<GuestLoginResponse>.Fail(
                $"회원으로 전환된 계정입니다. 일반 로그인을 이용해주세요.|{user.LoginId}");
        }

        // Convention 정보 조회
        var convention = await _unitOfWork.Conventions.GetByIdAsync(request.ConventionId);
        if (convention == null)
        {
            _logger.LogWarning("Guest login failed: Convention {ConventionId} not found", request.ConventionId);
            return AuthResult<GuestLoginResponse>.Fail(
                "참석자 또는 행사 정보를 찾을 수 없습니다.",
                AuthErrorType.Unauthorized);
        }

        var accessToken = GenerateAccessToken(user, user.Id, userConvention.ConventionId);

        return AuthResult<GuestLoginResponse>.Success(new GuestLoginResponse
        {
            AccessToken = accessToken,
            IsGuest = true,
            User = new GuestUserInfo
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                CorpPart = user.CorpPart,
                Affiliation = user.Affiliation
            },
            Convention = new GuestConventionInfo
            {
                Id = convention.Id,
                Title = convention.Title,
                StartDate = convention.StartDate,
                EndDate = convention.EndDate
            }
        });
    }

    public async Task<AuthResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // RefreshToken으로 조회 (AsNoTracking)
        var user = await _unitOfWork.Users.GetAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            return AuthResult<RefreshTokenResponse>.Fail(
                "유효하지 않은 리프레시 토큰입니다.",
                AuthErrorType.Unauthorized);
        }

        // tracked 엔티티로 다시 조회하여 업데이트
        var trackedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
        if (trackedUser == null)
        {
            return AuthResult<RefreshTokenResponse>.Fail(
                "유효하지 않은 리프레시 토큰입니다.",
                AuthErrorType.Unauthorized);
        }

        var accessToken = GenerateAccessToken(trackedUser);
        var refreshToken = GenerateRefreshToken();

        trackedUser.RefreshToken = refreshToken;
        trackedUser.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _unitOfWork.SaveChangesAsync();

        return AuthResult<RefreshTokenResponse>.Success(new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return AuthResult<CurrentUserResponse>.Fail(
                "사용자를 찾을 수 없습니다.",
                AuthErrorType.NotFound);
        }

        // UserConvention 목록 조회 (Convention 포함)
        var userConventions = (await _unitOfWork.UserConventions.GetByUserIdAsync(userId)).ToList();

        var conventionList = new List<UserConventionInfo>();
        foreach (var uc in userConventions.Where(uc => uc.Convention != null))
        {
            conventionList.Add(new UserConventionInfo
            {
                Id = uc.ConventionId,
                Title = uc.Convention!.Title,
                StartDate = uc.Convention.StartDate,
                EndDate = uc.Convention.EndDate,
                UserId = user.Id,
                Name = user.Name,
                UnreadCount = 0
            });
        }

        // 첫 번째 행사의 체크리스트 상태
        object? checklistStatus = null;
        var firstUserConvention = userConventions.FirstOrDefault();

        _logger.LogInformation(
            "GetCurrentUser: UserId={UserId}, ConventionCount={ConventionCount}, FirstConvention={FirstConventionId}",
            user.Id, userConventions.Count, firstUserConvention?.ConventionId ?? 0);

        if (firstUserConvention != null)
        {
            checklistStatus = await _checklistService.BuildChecklistStatusAsync(user.Id, firstUserConvention.ConventionId);
            _logger.LogInformation("Checklist status built: {HasStatus}", checklistStatus != null);
        }

        return AuthResult<CurrentUserResponse>.Success(new CurrentUserResponse
        {
            Id = user.Id,
            LoginId = user.LoginId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
            Conventions = conventionList,
            ChecklistStatus = checklistStatus
        });
    }

    public async Task<IEnumerable<AvailableConventionInfo>> GetAvailableConventionsAsync(int userId)
    {
        var joinedConventionIds = (await _unitOfWork.UserConventions.GetByUserIdAsync(userId))
            .Select(uc => uc.ConventionId)
            .ToHashSet();

        var allActiveConventions = await _unitOfWork.Conventions.FindAsync(
            c => c.DeleteYn == DeleteStatus.Active);

        return allActiveConventions
            .Where(c => !joinedConventionIds.Contains(c.Id))
            .Select(c => new AvailableConventionInfo
            {
                Id = c.Id,
                Title = c.Title,
                ConventionType = c.ConventionType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ConventionImg = c.ConventionImg
            })
            .ToList();
    }
}
