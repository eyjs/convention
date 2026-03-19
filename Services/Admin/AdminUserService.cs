using LocalRAG.Constants;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocalRAG.Services.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<AdminUserService> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<object> GetGuestsAsync(int conventionId)
    {
        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestAttributes)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
            .Include(uc => uc.User)
                .ThenInclude(u => u.UserOptionTours)
                    .ThenInclude(uot => uot.OptionTour)
            .OrderBy(uc => uc.User.Name)
            .Select(uc => new
            {
                Id = uc.UserId,
                GuestName = uc.User.Name,
                Name = uc.User.Name,
                Telephone = uc.User.Phone,
                Phone = uc.User.Phone,
                CorpPart = uc.User.CorpPart,
                ResidentNumber = uc.User.ResidentNumber,
                Affiliation = uc.User.Affiliation,
                GroupName = uc.GroupName,
                ConventionId = uc.ConventionId,
                AccessToken = uc.AccessToken,
                IsRegisteredUser = !string.IsNullOrEmpty(uc.User.LoginId) && !uc.User.LoginId.StartsWith("guest_"),
                user = new { uc.User.Id, uc.User.LoginId, uc.User.Role },
                scheduleTemplates = uc.User.GuestScheduleTemplates
                    .Where(gst => gst.ScheduleTemplate.ConventionId == conventionId)
                    .Select(gst => new
                    {
                        gst.ScheduleTemplateId,
                        gst.ScheduleTemplate!.CourseName
                    }).ToList(),
                attributes = uc.User.GuestAttributes
                    .Select(ga => new { ga.AttributeKey, ga.AttributeValue }).ToList(),
                optionTours = uc.User.UserOptionTours
                    .Where(uot => uot.ConventionId == conventionId)
                    .Select(uot => new
                    {
                        uot.OptionTourId,
                        uot.OptionTour!.Name
                    }).ToList(),
                passport = new
                {
                    HasNumber = !string.IsNullOrEmpty(uc.User.PassportNumber),
                    HasExpiry = uc.User.PassportExpiryDate != null,
                    HasImage = !string.IsNullOrEmpty(uc.User.PassportImageUrl),
                    uc.User.PassportVerified,
                    uc.User.PassportVerifiedAt,
                    uc.User.PassportExpiryDate
                }
            })
            .ToListAsync();

        return guests;
    }

    public async Task<object?> GetGuestDetailAsync(int guestId)
    {
        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestAttributes)
            .Include(u => u.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
                    .ThenInclude(st => st!.ScheduleItems.OrderBy(si => si.OrderNum))
            .FirstOrDefaultAsync(u => u.Id == guestId);

        if (user == null) return null;

        return new
        {
            Id = user.Id,
            GuestName = user.Name,
            Name = user.Name,
            Telephone = user.Phone,
            Phone = user.Phone,
            CorpPart = user.CorpPart,
            ResidentNumber = user.ResidentNumber,
            Affiliation = user.Affiliation,
            AccessToken = (string?)null,
            IsRegisteredUser = !string.IsNullOrEmpty(user.LoginId) && !user.LoginId.StartsWith("guest_"),
            user = new { user.Id, user.LoginId, user.Role },
            schedules = user.GuestScheduleTemplates.Select(gst => new
            {
                gst.ScheduleTemplateId,
                gst.ScheduleTemplate!.CourseName,
                gst.ScheduleTemplate.Description,
                items = gst.ScheduleTemplate.ScheduleItems.Select(si => new
                {
                    si.Id, si.ScheduleDate, si.StartTime, si.EndTime,
                    si.Title, si.Content, si.Location, si.OrderNum
                }).ToList()
            }).ToList(),
            attributes = user.GuestAttributes
                .ToDictionary(ga => ga.AttributeKey, ga => ga.AttributeValue)
        };
    }

    public async Task<(bool Success, object Result, int StatusCode)> CreateGuestAsync(int conventionId, UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, new { field = "name", message = "이름을 입력해주세요." }, 400);

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return (false, new { field = "phone", message = "전화번호를 입력해주세요." }, 400);

        // 동일 전화번호의 기존 사용자가 이미 해당 컨벤션에 등록되어 있는지 확인
        var existingUser = await _unitOfWork.Users.Query
            .FirstOrDefaultAsync(u => u.Phone == dto.Phone.Trim());

        if (existingUser != null)
        {
            var alreadyLinked = await _unitOfWork.UserConventions.Query
                .AnyAsync(uc => uc.UserId == existingUser.Id && uc.ConventionId == conventionId);
            if (alreadyLinked)
                return (false, new { message = "이미 해당 행사에 등록된 참석자입니다." }, 409);
        }

        var passwordToSet = ResolveGuestPassword(dto);

        User user;
        if (existingUser != null)
        {
            user = existingUser;
            user.Name = dto.Name.Trim();
            user.CorpPart = dto.CorpPart?.Trim();
            user.ResidentNumber = dto.ResidentNumber?.Trim();
            user.Affiliation = dto.Affiliation?.Trim();
            user.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            user = new User
            {
                LoginId = $"guest_{Guid.NewGuid():N}",
                PasswordHash = _authService.HashPassword(passwordToSet),
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                CorpPart = dto.CorpPart?.Trim(),
                ResidentNumber = dto.ResidentNumber?.Trim(),
                Affiliation = dto.Affiliation?.Trim(),
                Role = Roles.Guest,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        var userConvention = new UserConvention
        {
            UserId = user.Id,
            ConventionId = conventionId,
            AccessToken = GenerateAccessToken(),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserConventions.AddAsync(userConvention);
        await _unitOfWork.SaveChangesAsync();

        if (dto.Attributes != null)
        {
            foreach (var attr in dto.Attributes)
            {
                await _unitOfWork.GuestAttributes.AddAsync(new GuestAttribute
                {
                    UserId = user.Id,
                    AttributeKey = attr.Key,
                    AttributeValue = attr.Value
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return (true, new { Id = user.Id, Name = user.Name, message = "비회원 참석자가 성공적으로 생성되었습니다." }, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> UpdateGuestAsync(int id, UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, new { field = "name", message = "이름을 입력해주세요." }, 400);

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return (false, new { field = "phone", message = "전화번호를 입력해주세요." }, 400);

        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestAttributes)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return (false, new { message = "참석자를 찾을 수 없습니다." }, 404);

        user.Name = dto.Name.Trim();
        user.Phone = dto.Phone.Trim();
        user.CorpPart = dto.CorpPart?.Trim();
        user.ResidentNumber = dto.ResidentNumber?.Trim();
        user.Affiliation = dto.Affiliation?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _authService.HashPassword(dto.Password);
        }

        if (dto.Attributes != null)
        {
            var existingAttrs = user.GuestAttributes.ToList();
            _unitOfWork.GuestAttributes.RemoveRange(existingAttrs);

            foreach (var attr in dto.Attributes)
            {
                await _unitOfWork.GuestAttributes.AddAsync(new GuestAttribute
                {
                    UserId = user.Id,
                    AttributeKey = attr.Key,
                    AttributeValue = attr.Value
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, new { Id = user.Id, Name = user.Name }, 200);
    }

    public async Task<(bool Found, int StatusCode)> DeleteGuestAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return (false, 404);

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
        return (true, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> ConvertToUserAsync(int guestId, CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LoginId))
            return (false, new { field = "loginId", message = "로그인 ID를 입력해주세요." }, 400);

        if (string.IsNullOrWhiteSpace(dto.Password))
            return (false, new { field = "password", message = "비밀번호를 입력해주세요." }, 400);

        if (dto.Password.Length < 6)
            return (false, new { field = "password", message = "비밀번호는 최소 6자 이상이어야 합니다." }, 400);

        var user = await _unitOfWork.Users.GetByIdAsync(guestId);
        if (user == null)
            return (false, new { message = "참석자를 찾을 수 없습니다." }, 404);

        if (!string.IsNullOrEmpty(user.LoginId) && !user.LoginId.StartsWith("guest_"))
            return (false, new { message = "이미 회원입니다." }, 400);

        var existing = await _unitOfWork.Users.Query
            .FirstOrDefaultAsync(u => u.LoginId == dto.LoginId && u.Id != guestId);
        if (existing != null)
            return (false, new { field = "loginId", message = "이미 사용 중인 로그인 ID입니다." }, 400);

        user.LoginId = dto.LoginId;
        user.PasswordHash = _authService.HashPassword(dto.Password);
        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "회원으로 전환되었습니다.", userId = user.Id }, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> UpdateUserRoleAsync(int guestId, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(guestId);
        if (user == null)
            return (false, new { message = "계정을 찾을 수 없습니다." }, 404);

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, new { message = "권한이 수정되었습니다." }, 200);
    }

    public async Task<object> GetUsersAsync(string? searchTerm, string? role, int page, int pageSize)
    {
        var query = _unitOfWork.Users.Query;

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                (u.Name != null && u.Name.ToLower().Contains(term)) ||
                (u.LoginId != null && u.LoginId.ToLower().Contains(term)) ||
                (u.Phone != null && u.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id, u.Name, u.LoginId, u.Phone,
                u.Role, u.IsActive, u.CreatedAt, u.UpdatedAt
            })
            .ToListAsync();

        return new
        {
            Items = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<(bool Found, object? Result, int StatusCode)> ToggleUserStatusAsync(int id, UpdateUserStatusDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return (false, null, 404);

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = $"사용자 상태가 {(dto.IsActive ? "활성" : "비활성")}으로 변경되었습니다." }, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> UpdateUserRoleDirectAsync(int id, UpdateUserRoleDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return (false, new { message = "사용자를 찾을 수 없습니다." }, 404);

        if (!Roles.IsValid(dto.Role))
            return (false, new { message = "유효하지 않은 역할입니다. 'Admin', 'User', 'Guest' 중 하나여야 합니다." }, 400);

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = $"사용자 역할이 '{dto.Role}'(으)로 변경되었습니다." }, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> ResetPasswordAsync(int userId, ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return (false, new { message = "비밀번호를 입력해주세요." }, 400);

        if (dto.NewPassword.Length < 6)
            return (false, new { message = "비밀번호는 최소 6자 이상이어야 합니다." }, 400);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, new { message = "사용자를 찾을 수 없습니다." }, 404);

        user.PasswordHash = _authService.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return (true, new { message = "비밀번호가 재설정되었습니다." }, 200);
    }

    public async Task<(bool Found, object? Result)> GetAccessLinkAsync(int guestId, int? conventionId, string baseUrl)
    {
        var query = _unitOfWork.UserConventions.Query
            .Include(uc => uc.Convention)
            .Where(uc => uc.UserId == guestId);

        if (conventionId.HasValue)
            query = query.Where(uc => uc.ConventionId == conventionId.Value);

        var userConvention = await query.FirstOrDefaultAsync();
        if (userConvention == null) return (false, null);

        var link = $"{baseUrl}/guest/{userConvention.Convention.Id}/{userConvention.AccessToken}";

        return (true, new { link, accessToken = userConvention.AccessToken });
    }

    public async Task<(bool Success, object Result, int StatusCode)> LinkExistingUsersAsync(int conventionId, List<int> userIds, string? groupName)
    {
        if (userIds == null || userIds.Count == 0)
            return (false, new { message = "추가할 사용자를 선택해주세요." }, 400);

        var conventionExists = await _unitOfWork.Conventions.Query
            .AnyAsync(c => c.Id == conventionId);
        if (!conventionExists)
            return (false, new { message = "행사를 찾을 수 없습니다." }, 404);

        var users = await _unitOfWork.Users.Query
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count == 0)
            return (false, new { message = "사용자를 찾을 수 없습니다." }, 404);

        var existingLinks = await _unitOfWork.UserConventions.Query
            .Where(uc => userIds.Contains(uc.UserId) && uc.ConventionId == conventionId)
            .Select(uc => uc.UserId)
            .ToListAsync();

        var newUserIds = userIds.Except(existingLinks).ToList();
        var skippedCount = userIds.Count - newUserIds.Count;

        foreach (var uid in newUserIds)
        {
            await _unitOfWork.UserConventions.AddAsync(new UserConvention
            {
                UserId = uid,
                ConventionId = conventionId,
                GroupName = groupName,
                AccessToken = GenerateAccessToken(),
                CreatedAt = DateTime.UtcNow
            });
        }

        if (newUserIds.Count > 0)
            await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            addedCount = newUserIds.Count,
            skippedCount,
            message = skippedCount > 0
                ? $"{newUserIds.Count}명 추가, {skippedCount}명 이미 등록됨"
                : $"{newUserIds.Count}명이 참석자로 추가되었습니다."
        }, 200);
    }

    public async Task<(bool Success, object Result, int StatusCode)> TogglePassportVerificationAsync(int userId, bool verified)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, new { message = "사용자를 찾을 수 없습니다." }, 404);

        user.PassportVerified = verified;
        user.PassportVerifiedAt = verified ? DateTime.UtcNow : null;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            message = verified ? "여권 정보가 검증 완료되었습니다." : "여권 검증이 해제되었습니다.",
            passportVerified = user.PassportVerified,
            passportVerifiedAt = user.PassportVerifiedAt
        }, 200);
    }

    private static string ResolveGuestPassword(UserDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Password))
            return dto.Password;
        if (!string.IsNullOrWhiteSpace(dto.ResidentNumber) && dto.ResidentNumber.Length >= 6)
            return dto.ResidentNumber[..6];
        return "123456";
    }

    private static string GenerateAccessToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}
