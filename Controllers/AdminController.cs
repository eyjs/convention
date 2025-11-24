using LocalRAG.Data;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Entities;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.DTOs.AdminModels;
using UserEntity = LocalRAG.Entities.User;
using OfficeOpenXml;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IAuthService _authService;
    private readonly IFileUploadService _fileUploadService;

    public AdminController(ConventionDbContext context, IAuthService authService, IFileUploadService fileUploadService)
    {
        _context = context;
        _authService = authService;
        _fileUploadService = fileUploadService;
    }

    [HttpPost("conventions/upload-cover-image")]
    public async Task<IActionResult> UploadConventionCoverImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "파일이 제공되지 않았습니다." });

            var uploadResult = await _fileUploadService.UploadImageAsync(file, "convention-covers");

            if (string.IsNullOrEmpty(uploadResult.Url))
            {
                return StatusCode(500, new { message = "커버 이미지 업로드에 실패했습니다." });
            }

            return Ok(new { url = uploadResult.Url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // In a real app, use a logger here
            Console.WriteLine($"Convention cover image upload failed: {ex}");
            return StatusCode(500, new { message = "이미지 업로드 중 오류가 발생했습니다.", error = ex.Message });
        }
    }

    [HttpGet("conventions/{conventionId}/stats")]
    public async Task<IActionResult> GetStats(int conventionId)
    {
        var totalGuests = await _context.UserConventions.CountAsync(uc => uc.ConventionId == conventionId);
        var totalSchedules = await _context.ScheduleTemplates.CountAsync(st => st.ConventionId == conventionId);
        var scheduleAssignments = await _context.Set<GuestScheduleTemplate>()
            .Include(gst => gst.User)
                .ThenInclude(u => u.UserConventions)
            .CountAsync(gst => gst.User!.UserConventions.Any(uc => uc.ConventionId == conventionId));

        var recentGuests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .OrderByDescending(uc => uc.UserId)
            .Take(5)
            .Select(uc => new { Id = uc.UserId, Name = uc.User.Name, CorpPart = uc.User.CorpPart, Phone = uc.User.Phone })
            .ToListAsync();

        var scheduleStats = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .Select(st => new
            {
                st.Id,
                st.CourseName,
                ItemCount = st.ScheduleItems.Count,
                GuestCount = st.GuestScheduleTemplates.Count
            })
            .ToListAsync();

        var attributeStats = await _context.Users
            .Where(u => u.UserConventions.Any(uc => uc.ConventionId == conventionId))
            .SelectMany(u => u.GuestAttributes)
            .GroupBy(ga => ga.AttributeKey)
            .Select(group => new
            {
                AttributeKey = group.Key,
                Values = group.GroupBy(ga => ga.AttributeValue)
                    .Select(vg => new { Value = vg.Key, Count = vg.Count() })
                    .OrderByDescending(v => v.Count)
                    .ToList(),
                TotalCount = group.Count()
            })
            .OrderByDescending(a => a.TotalCount)
            .ToListAsync();

        return Ok(new { totalGuests, totalSchedules, scheduleAssignments, recentGuests, scheduleStats, attributeStats });
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
                GuestName = uc.User.Name, // 프론트엔드 호환성
                Name = uc.User.Name,
                Telephone = uc.User.Phone, // 프론트엔드 호환성
                Phone = uc.User.Phone,
                CorpPart = uc.User.CorpPart,
                ResidentNumber = uc.User.ResidentNumber,
                Affiliation = uc.User.Affiliation,
                GroupName = uc.GroupName, // UserConvention의 그룹명
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
                attributes = uc.User.GuestAttributes.Select(ga => new { ga.AttributeKey, ga.AttributeValue }).ToList()
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
            GuestName = user.Name, // 프론트엔드 호환성
            Name = user.Name,
            Telephone = user.Phone, // 프론트엔드 호환성
            Phone = user.Phone,
            CorpPart = user.CorpPart,
            ResidentNumber = user.ResidentNumber,
            Affiliation = user.Affiliation,
            AccessToken = (string?)null, // AccessToken is now in UserConvention, need convention context
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
            attributes = user.GuestAttributes.ToDictionary(ga => ga.AttributeKey, ga => ga.AttributeValue)
        });
    }

    [HttpPost("conventions/{conventionId}/guests")]
    public async Task<IActionResult> CreateGuest(int conventionId, [FromBody] UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { field = "name", message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest(new { field = "phone", message = "전화번호를 입력해주세요." });

        string passwordToSet;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            passwordToSet = dto.Password;
        else if (!string.IsNullOrWhiteSpace(dto.ResidentNumber) && dto.ResidentNumber.Length >= 6)
            passwordToSet = dto.ResidentNumber.Substring(0, 6);
        else
            passwordToSet = "123456";

        // Check if user with this phone already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone.Trim());

        UserEntity user;
        if (existingUser != null)
        {
            user = existingUser;
            // Update user info if needed
            user.Name = dto.Name.Trim();
            user.CorpPart = dto.CorpPart?.Trim();
            user.ResidentNumber = dto.ResidentNumber?.Trim();
            user.Affiliation = dto.Affiliation?.Trim();
            user.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new user (guest without login)
            user = new UserEntity
            {
                LoginId = string.Empty, // No login ID for guests
                PasswordHash = _authService.HashPassword(passwordToSet),
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                CorpPart = dto.CorpPart?.Trim(),
                ResidentNumber = dto.ResidentNumber?.Trim(),
                Affiliation = dto.Affiliation?.Trim(),
                Role = "Guest",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Create UserConvention mapping
        var userConvention = new UserConvention
        {
            UserId = user.Id,
            ConventionId = conventionId,
            AccessToken = GenerateAccessToken(),
            CreatedAt = DateTime.UtcNow
        };
        _context.UserConventions.Add(userConvention);
        await _context.SaveChangesAsync();

        // Add attributes if provided
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
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Users
            .Where(u => u.Role == "Admin" || u.Role == "User")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u => 
                (u.Name != null && u.Name.ToLower().Contains(term)) ||
                (u.LoginId != null && u.LoginId.ToLower().Contains(term)) ||
                (u.Phone != null && u.Phone.Contains(term))
            );
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new 
            {
                u.Id,
                u.Name,
                u.LoginId,
                u.Phone,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt
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
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"사용자 상태가 {(dto.IsActive ? "활성" : "비활성")}으로 변경되었습니다." });
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        if (string.IsNullOrWhiteSpace(dto.Role) || (dto.Role != "Admin" && dto.Role != "User" && dto.Role != "Guest"))
        {
            return BadRequest(new { message = "유효하지 않은 역할입니다. 'Admin', 'User', 'Guest' 중 하나여야 합니다." });
        }

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"사용자 역할이 '{dto.Role}'(으)로 변경되었습니다." });
    }


    [HttpPost("conventions/{conventionId}/guests/{guestId}/schedules")]
    public async Task<IActionResult> AssignSchedules(int conventionId, int guestId, [FromBody] AssignSchedulesDto dto)
    {
        var user = await _context.Users.FindAsync(guestId);
        if (user == null) return NotFound();

        // This is the critical fix: Filter by ConventionId as well.
        var existing = await _context.Set<GuestScheduleTemplate>()
            .Where(gst => gst.UserId == guestId && gst.ScheduleTemplate.ConventionId == conventionId)
            .ToListAsync();
        _context.Set<GuestScheduleTemplate>().RemoveRange(existing);

        foreach (var templateId in dto.ScheduleTemplateIds)
        {
            _context.Set<GuestScheduleTemplate>().Add(new GuestScheduleTemplate
            {
                UserId = guestId,
                ScheduleTemplateId = templateId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "일정이 배정되었습니다." });
    }

    [HttpDelete("guests/{id}")]
    public async Task<IActionResult> DeleteGuest(int id)
    {
        // Note: This deletes the user entirely. Consider if you only want to remove UserConvention instead.
        // To only remove from convention: delete UserConvention record
        // To remove user entirely: delete User (cascades will handle related data)

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

        var existing = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == dto.LoginId && u.Id != guestId);
        if (existing != null)
            return BadRequest(new { field = "loginId", message = "이미 사용 중인 로그인 ID입니다." });

        // Convert guest to registered user
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
        // Note: AccessToken is now in UserConvention, so we need a conventionId
        // If conventionId is not provided, return first convention's token
        var query = _context.UserConventions
            .Where(uc => uc.UserId == userId);

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

    [HttpGet("conventions/{conventionId}/schedule-templates")]
    public async Task<IActionResult> GetScheduleTemplates(int conventionId)
    {
        var templates = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .Include(st => st.ScheduleItems)
            .Include(st => st.GuestScheduleTemplates)
            .OrderBy(st => st.OrderNum)
            .Select(st => new
            {
                st.Id, st.CourseName, st.Description, st.OrderNum, st.CreatedAt,
                GuestCount = st.GuestScheduleTemplates.Count,
                ScheduleItems = st.ScheduleItems
                    .OrderBy(si => si.ScheduleDate)
                    .ThenBy(si => si.StartTime)
                    .Select(si => new
                    {
                        si.Id, si.ScheduleDate, si.StartTime, si.EndTime,
                        si.Title, si.Location, si.Content, si.OrderNum
                    }).ToList()
            })
            .ToListAsync();

        return Ok(templates);
    }

    [HttpPost("conventions/{conventionId}/schedule-templates")]
    public async Task<IActionResult> CreateScheduleTemplate(int conventionId, [FromBody] ScheduleTemplateDto dto)
    {
        var template = new ScheduleTemplate
        {
            ConventionId = conventionId,
            CourseName = dto.CourseName,
            Description = dto.Description,
            OrderNum = dto.OrderNum
        };

        _context.ScheduleTemplates.Add(template);
        await _context.SaveChangesAsync();

        return Ok(template);
    }

    [HttpPut("schedule-templates/{id}")]
    public async Task<IActionResult> UpdateScheduleTemplate(int id, [FromBody] ScheduleTemplateDto dto)
    {
        var template = await _context.ScheduleTemplates.FindAsync(id);
        if (template == null) return NotFound();

        template.CourseName = dto.CourseName;
        template.Description = dto.Description;
        template.OrderNum = dto.OrderNum;

        await _context.SaveChangesAsync();
        return Ok(template);
    }

    [HttpDelete("schedule-templates/{id}")]
    public async Task<IActionResult> DeleteScheduleTemplate(int id)
    {
        var template = await _context.ScheduleTemplates
            .Include(st => st.ScheduleItems)
            .Include(st => st.GuestScheduleTemplates)
            .FirstOrDefaultAsync(st => st.Id == id);
            
        if (template == null) return NotFound();

        if (template.GuestScheduleTemplates.Any())
            _context.Set<GuestScheduleTemplate>().RemoveRange(template.GuestScheduleTemplates);

        if (template.ScheduleItems.Any())
            _context.ScheduleItems.RemoveRange(template.ScheduleItems);

        _context.ScheduleTemplates.Remove(template);
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [HttpPost("schedule-items")]
    public async Task<IActionResult> CreateScheduleItem([FromBody] ScheduleItemDto dto)
    {
        var item = new ScheduleItem
        {
            ScheduleTemplateId = dto.ScheduleTemplateId,
            ScheduleDate = dto.ScheduleDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Title = dto.Title,
            Location = dto.Location,
            Content = dto.Content,
            OrderNum = dto.OrderNum
        };

        _context.ScheduleItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpPut("schedule-items/{id}")]
    public async Task<IActionResult> UpdateScheduleItem(int id, [FromBody] ScheduleItemDto dto)
    {
        var item = await _context.ScheduleItems.FindAsync(id);
        if (item == null) return NotFound();

        item.ScheduleDate = dto.ScheduleDate;
        item.StartTime = dto.StartTime;
        item.EndTime = dto.EndTime;
        item.Title = dto.Title;
        item.Location = dto.Location;
        item.Content = dto.Content;
        item.OrderNum = dto.OrderNum;

        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("schedule-items/{id}")]
    public async Task<IActionResult> DeleteScheduleItem(int id)
    {
        var item = await _context.ScheduleItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.ScheduleItems.Remove(item);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("schedule-items/bulk")]
    public async Task<IActionResult> CreateScheduleItemsBulk([FromBody] BulkScheduleItemsDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(new { message = "복사할 일정이 없습니다." });

        var items = dto.Items.Select(itemDto => new ScheduleItem
        {
            ScheduleTemplateId = itemDto.ScheduleTemplateId,
            ScheduleDate = itemDto.ScheduleDate,
            StartTime = itemDto.StartTime,
            EndTime = itemDto.EndTime,
            Title = itemDto.Title,
            Location = itemDto.Location,
            Content = itemDto.Content,
            OrderNum = itemDto.OrderNum
        }).ToList();

        _context.ScheduleItems.AddRange(items);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{items.Count}개 일정이 추가되었습니다.", count = items.Count });
    }

    [HttpGet("schedule-templates/{templateId}/guests")]
    public async Task<IActionResult> GetTemplateGuests(int templateId)
    {
        var guests = await _context.Set<GuestScheduleTemplate>()
            .Where(gst => gst.ScheduleTemplateId == templateId)
            .Include(gst => gst.User)
            .Select(gst => new
            {
                Id = gst.User!.Id,
                Name = gst.User.Name,
                Phone = gst.User.Phone,
                CorpPart = gst.User.CorpPart,
                Affiliation = gst.User.Affiliation,
                AssignedAt = gst.AssignedAt
            })
            .ToListAsync();

        return Ok(guests);
    }

    [HttpDelete("guests/{guestId}/schedules/{templateId}")]
    public async Task<IActionResult> RemoveGuestSchedule(int userId, int templateId)
    {
        var assignment = await _context.Set<GuestScheduleTemplate>()
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == templateId);

        if (assignment == null) return NotFound();

        _context.Set<GuestScheduleTemplate>().Remove(assignment);
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// 참석자의 특정 속성 삭제
    /// </summary>
    [HttpDelete("guests/{guestId}/attributes/{attributeKey}")]
    public async Task<IActionResult> DeleteGuestAttribute(int guestId, string attributeKey)
    {
        var decodedKey = Uri.UnescapeDataString(attributeKey);

        var attribute = await _context.GuestAttributes
            .FirstOrDefaultAsync(ga => ga.UserId == guestId && ga.AttributeKey == decodedKey);

        if (attribute == null)
            return NotFound(new { message = $"속성 '{decodedKey}'을 찾을 수 없습니다." });

        _context.GuestAttributes.Remove(attribute);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"속성 '{decodedKey}'이 삭제되었습니다." });
    }

    [HttpGet("conventions/{conventionId}/schedules")]
    public async Task<IActionResult> GetAllSchedules(int conventionId)
    {
        var items = await _context.ScheduleItems
            .Include(si => si.ScheduleTemplate)
            .Where(si => si.ScheduleTemplate!.ConventionId == conventionId)
            .OrderBy(si => si.ScheduleDate)
            .ThenBy(si => si.StartTime)
            .Select(si => new
            {
                si.Id, si.ScheduleDate, si.StartTime, si.EndTime,
                si.Title, si.Location, si.Content, si.OrderNum,
                Templates = new[] { new { si.ScheduleTemplate!.Id, si.ScheduleTemplate.CourseName } }
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// 참석자 목록을 엑셀로 다운로드 (속성 업로드용 템플릿)
    /// A열: User ID, B열: 이름, C열: 전화번호, D열부터: 기존 속성들
    /// </summary>
    [HttpGet("conventions/{conventionId}/guests/download")]
    public async Task<IActionResult> DownloadGuestsForAttributeUpload(int conventionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var guests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestAttributes)
            .OrderBy(uc => uc.User.Name)
            .ToListAsync();

        if (!guests.Any())
            return NotFound(new { message = "참석자가 없습니다." });

        // 모든 속성 키 수집 (헤더용)
        var allAttributeKeys = guests
            .SelectMany(uc => uc.User.GuestAttributes.Select(ga => ga.AttributeKey))
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("참석자속성");

        // 헤더 작성
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "이름";
        sheet.Cells[1, 3].Value = "전화번호";

        for (int i = 0; i < allAttributeKeys.Count; i++)
        {
            sheet.Cells[1, 4 + i].Value = allAttributeKeys[i];
        }

        // 헤더 스타일
        using (var headerRange = sheet.Cells[1, 1, 1, 3 + allAttributeKeys.Count])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(255, 211, 211, 211); // LightGray (ARGB)
        }

        // 데이터 작성
        int row = 2;
        foreach (var uc in guests)
        {
            sheet.Cells[row, 1].Value = uc.UserId;
            sheet.Cells[row, 2].Value = uc.User.Name;
            sheet.Cells[row, 3].Value = uc.User.Phone;

            // 기존 속성 값 채우기
            for (int i = 0; i < allAttributeKeys.Count; i++)
            {
                var attributeKey = allAttributeKeys[i];
                var attribute = uc.User.GuestAttributes.FirstOrDefault(ga => ga.AttributeKey == attributeKey);
                if (attribute != null)
                {
                    sheet.Cells[row, 4 + i].Value = attribute.AttributeValue;
                }
            }

            row++;
        }

        // 열 너비 자동 조정
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        var fileBytes = package.GetAsByteArray();
        var fileName = $"참석자속성_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private string GenerateAccessToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}

