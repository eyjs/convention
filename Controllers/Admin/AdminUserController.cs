using LocalRAG.Data;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Entities;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Constants;
using UserEntity = LocalRAG.Entities.User;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminUserController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IAuthService _authService;

    public AdminUserController(ConventionDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet("conventions/{conventionId}/guests")]
    public async Task<IActionResult> GetGuests(int conventionId)
    {
        var guests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestAttributes)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
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
                IsRegisteredUser = !string.IsNullOrEmpty(uc.User.LoginId),
                user = new { uc.User.Id, uc.User.LoginId, uc.User.Role },
                scheduleTemplates = uc.User.GuestScheduleTemplates
                    .Where(gst => gst.ScheduleTemplate.ConventionId == conventionId)
                    .Select(gst => new
                    {
                        gst.ScheduleTemplateId,
                        gst.ScheduleTemplate!.CourseName
                    }).ToList(),
                attributes = uc.User.GuestAttributes
                    .Select(ga => new { ga.AttributeKey, ga.AttributeValue }).ToList()
            })
            .ToListAsync();

        return Ok(guests);
    }

    [HttpGet("guests/{guestId}/detail")]
    public async Task<IActionResult> GetGuestDetail(int guestId)
    {
        var user = await _context.Users
            .Include(u => u.GuestAttributes)
            .Include(u => u.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
                    .ThenInclude(st => st!.ScheduleItems.OrderBy(si => si.OrderNum))
            .FirstOrDefaultAsync(u => u.Id == guestId);

        if (user == null) return NotFound();

        return Ok(new
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
            IsRegisteredUser = !string.IsNullOrEmpty(user.LoginId),
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
        });
    }

    [HttpPost("conventions/{conventionId}/guests")]
    public async Task<IActionResult> CreateGuest(int conventionId, [FromBody] UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { field = "name", message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest(new { field = "phone", message = "전화번호를 입력해주세요." });

        var passwordToSet = ResolveGuestPassword(dto);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == dto.Phone.Trim());

        UserEntity user;
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
            user = new UserEntity
            {
                LoginId = string.Empty,
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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var userConvention = new UserConvention
        {
            UserId = user.Id,
            ConventionId = conventionId,
            AccessToken = GenerateAccessToken(),
            CreatedAt = DateTime.UtcNow
        };
        _context.UserConventions.Add(userConvention);
        await _context.SaveChangesAsync();

        if (dto.Attributes != null)
        {
            foreach (var attr in dto.Attributes)
            {
                _context.Set<GuestAttribute>().Add(new GuestAttribute
                {
                    UserId = user.Id,
                    AttributeKey = attr.Key,
                    AttributeValue = attr.Value
                });
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { Id = user.Id, Name = user.Name, message = "비회원 참석자가 성공적으로 생성되었습니다." });
    }

    [HttpPut("guests/{id}")]
    public async Task<IActionResult> UpdateGuest(int id, [FromBody] UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { field = "name", message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest(new { field = "phone", message = "전화번호를 입력해주세요." });

        var user = await _context.Users
            .Include(u => u.GuestAttributes)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

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
            _context.Set<GuestAttribute>().RemoveRange(existingAttrs);

            foreach (var attr in dto.Attributes)
            {
                _context.Set<GuestAttribute>().Add(new GuestAttribute
                {
                    UserId = user.Id,
                    AttributeKey = attr.Key,
                    AttributeValue = attr.Value
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { Id = user.Id, Name = user.Name });
    }

    [HttpDelete("guests/{id}")]
    public async Task<IActionResult> DeleteGuest(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("guests/{guestId}/convert-to-user")]
    public async Task<IActionResult> ConvertToUser(int guestId, [FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LoginId))
            return BadRequest(new { field = "loginId", message = "로그인 ID를 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { field = "password", message = "비밀번호를 입력해주세요." });

        if (dto.Password.Length < 6)
            return BadRequest(new { field = "password", message = "비밀번호는 최소 6자 이상이어야 합니다." });

        var user = await _context.Users.FindAsync(guestId);
        if (user == null) return NotFound(new { message = "참석자를 찾을 수 없습니다." });

        if (!string.IsNullOrEmpty(user.LoginId))
            return BadRequest(new { message = "이미 회원입니다." });

        var existing = await _context.Users
            .FirstOrDefaultAsync(u => u.LoginId == dto.LoginId && u.Id != guestId);
        if (existing != null)
            return BadRequest(new { field = "loginId", message = "이미 사용 중인 로그인 ID입니다." });

        user.LoginId = dto.LoginId;
        user.PasswordHash = _authService.HashPassword(dto.Password);
        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "회원으로 전환되었습니다.", userId = user.Id });
    }

    [HttpPut("guests/{guestId}/user")]
    public async Task<IActionResult> UpdateUserForGuest(int userId, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "계정을 찾을 수 없습니다." });

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "권한이 수정되었습니다." });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Users.AsQueryable();

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

        return Ok(new
        {
            Items = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"사용자 상태가 {(dto.IsActive ? "활성" : "비활성")}으로 변경되었습니다." });
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (!Roles.IsValid(dto.Role))
            return BadRequest(new { message = "유효하지 않은 역할입니다. 'Admin', 'User', 'Guest' 중 하나여야 합니다." });

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"사용자 역할이 '{dto.Role}'(으)로 변경되었습니다." });
    }

    [HttpPost("users/{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "비밀번호를 입력해주세요." });

        if (dto.NewPassword.Length < 6)
            return BadRequest(new { message = "비밀번호는 최소 6자 이상이어야 합니다." });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.PasswordHash = _authService.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "비밀번호가 재설정되었습니다." });
    }

    [HttpGet("guests/{guestId}/access-link")]
    public async Task<IActionResult> GetAccessLink(int userId, [FromQuery] int? conventionId = null)
    {
        var query = _context.UserConventions.Where(uc => uc.UserId == userId);

        if (conventionId.HasValue)
            query = query.Where(uc => uc.ConventionId == conventionId.Value);

        var userConvention = await query.FirstOrDefaultAsync();
        if (userConvention == null) return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var link = $"{baseUrl}/guest/{userConvention.Convention.Id}/{userConvention.AccessToken}";

        return Ok(new { link, accessToken = userConvention.AccessToken });
    }

    [HttpPost("guests/{guestId}/send-sms")]
    public async Task<IActionResult> SendSMS(int userId, [FromQuery] int? conventionId = null)
    {
        var query = _context.UserConventions
            .Include(uc => uc.Convention)
            .Include(uc => uc.User)
            .Where(uc => uc.UserId == userId);

        if (conventionId.HasValue)
            query = query.Where(uc => uc.ConventionId == conventionId.Value);

        var userConvention = await query.FirstOrDefaultAsync();
        if (userConvention == null) return NotFound();

        return Ok(new { message = "SMS 전송 기능은 추후 구현 예정입니다." });
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
