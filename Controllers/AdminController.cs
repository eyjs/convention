using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using GuestModel = LocalRAG.Models.Guest;
using LocalRAG.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IAuthService _authService;

    public AdminController(ConventionDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet("conventions/{conventionId}/stats")]
    public async Task<IActionResult> GetStats(int conventionId)
    {
        var totalGuests = await _context.Guests.CountAsync(g => g.ConventionId == conventionId);
        var totalSchedules = await _context.ScheduleTemplates.CountAsync(st => st.ConventionId == conventionId);
        var scheduleAssignments = await _context.Set<GuestScheduleTemplate>().CountAsync(gst => gst.Guest!.ConventionId == conventionId);

        var recentGuests = await _context.Guests
            .Where(g => g.ConventionId == conventionId)
            .OrderByDescending(g => g.Id)
            .Take(5)
            .Select(g => new { g.Id, g.GuestName, g.CorpPart, g.Telephone })
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

        var attributeStats = await _context.Guests
            .Where(g => g.ConventionId == conventionId)
            .SelectMany(g => g.GuestAttributes)
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
        var guests = await _context.Guests
            .Where(g => g.ConventionId == conventionId)
            .Include(g => g.User)
            .Include(g => g.GuestAttributes)
            .Include(g => g.GuestScheduleTemplates).ThenInclude(gst => gst.ScheduleTemplate)
            .OrderBy(g => g.GuestName)
            .Select(g => new
            {
                g.Id, g.GuestName, g.Telephone, g.CorpPart, g.ResidentNumber, g.Affiliation,
                g.ConventionId, g.AccessToken, g.IsRegisteredUser,
                user = g.User == null ? null : new { g.User.Id, g.User.LoginId, g.User.Role },
                scheduleTemplates = g.GuestScheduleTemplates.Select(gst => new
                {
                    gst.ScheduleTemplateId,
                    gst.ScheduleTemplate!.CourseName
                }).ToList(),
                attributes = g.GuestAttributes.Select(ga => new { ga.AttributeKey, ga.AttributeValue }).ToList()
            })
            .ToListAsync();

        return Ok(guests);
    }

    [HttpGet("guests/{guestId}/detail")]
    public async Task<IActionResult> GetGuestDetail(int guestId)
    {
        var guest = await _context.Guests
            .Include(g => g.User)
            .Include(g => g.GuestAttributes)
            .Include(g => g.GuestScheduleTemplates)
                .ThenInclude(gst => gst.ScheduleTemplate)
                    .ThenInclude(st => st!.ScheduleItems.OrderBy(si => si.OrderNum))
            .FirstOrDefaultAsync(g => g.Id == guestId);

        if (guest == null) return NotFound();

        return Ok(new
        {
            guest.Id, guest.GuestName, guest.Telephone, guest.CorpPart,
            guest.ResidentNumber, guest.Affiliation, guest.AccessToken, guest.IsRegisteredUser,
            user = guest.User == null ? null : new { guest.User.Id, guest.User.LoginId, guest.User.Role },
            schedules = guest.GuestScheduleTemplates.Select(gst => new
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
            attributes = guest.GuestAttributes.ToDictionary(ga => ga.AttributeKey, ga => ga.AttributeValue)
        });
    }

    [HttpPost("conventions/{conventionId}/guests")]
    public async Task<IActionResult> CreateGuest(int conventionId, [FromBody] GuestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.GuestName))
            return BadRequest(new { field = "guestName", message = "이름을 입력해주세요." });

        if (string.IsNullOrWhiteSpace(dto.Telephone))
            return BadRequest(new { field = "telephone", message = "전화번호를 입력해주세요." });

        string passwordToSet;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            passwordToSet = dto.Password;
        else if (!string.IsNullOrWhiteSpace(dto.ResidentNumber) && dto.ResidentNumber.Length >= 6)
            passwordToSet = dto.ResidentNumber.Substring(0, 6);
        else
            passwordToSet = "123456";

        var guest = new GuestModel
        {
            ConventionId = conventionId,
            GuestName = dto.GuestName.Trim(),
            Telephone = dto.Telephone.Trim(),
            CorpPart = dto.CorpPart?.Trim(),
            ResidentNumber = dto.ResidentNumber?.Trim(),
            Affiliation = dto.Affiliation?.Trim(),
            AccessToken = GenerateAccessToken(),
            IsRegisteredUser = false,
            PasswordHash = _authService.HashPassword(passwordToSet)
        };

        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();

        if (dto.Attributes != null)
        {
            foreach (var attr in dto.Attributes)
            {
                _context.Set<GuestAttribute>().Add(new GuestAttribute
                {
                    GuestId = guest.Id,
                    AttributeKey = attr.Key,
                    AttributeValue = attr.Value
                });
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { guest.Id, guest.GuestName, message = "비회원 참석자가 성공적으로 생성되었습니다." });
    }

    [HttpPut("guests/{id}")]
    public async Task<IActionResult> UpdateGuest(int id, [FromBody] GuestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.GuestName))
                return BadRequest(new { field = "guestName", message = "이름을 입력해주세요." });

            if (string.IsNullOrWhiteSpace(dto.Telephone))
                return BadRequest(new { field = "telephone", message = "전화번호를 입력해주세요." });

            var guest = await _context.Guests
                .Include(g => g.User)
                .Include(g => g.GuestAttributes)
                .FirstOrDefaultAsync(g => g.Id == id);
                
            if (guest == null) return NotFound();

            guest.GuestName = dto.GuestName.Trim();
            guest.Telephone = dto.Telephone.Trim();
            guest.CorpPart = dto.CorpPart?.Trim();
            guest.ResidentNumber = dto.ResidentNumber?.Trim();
            guest.Affiliation = dto.Affiliation?.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (guest.IsRegisteredUser && guest.User != null)
                {
                    guest.User.PasswordHash = _authService.HashPassword(dto.Password);
                    guest.User.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    guest.PasswordHash = _authService.HashPassword(dto.Password);
                }
            }

            if (dto.Attributes != null)
            {
                var existingAttrs = guest.GuestAttributes.ToList();
                _context.Set<GuestAttribute>().RemoveRange(existingAttrs);
                
                foreach (var attr in dto.Attributes)
                {
                    _context.Set<GuestAttribute>().Add(new GuestAttribute
                    {
                        GuestId = guest.Id,
                        AttributeKey = attr.Key,
                        AttributeValue = attr.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { guest.Id, guest.GuestName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpPost("guests/{guestId}/schedules")]
    public async Task<IActionResult> AssignSchedules(int guestId, [FromBody] AssignSchedulesDto dto)
    {
        var guest = await _context.Guests.FindAsync(guestId);
        if (guest == null) return NotFound();

        var existing = await _context.Set<GuestScheduleTemplate>()
            .Where(gst => gst.GuestId == guestId)
            .ToListAsync();
        _context.Set<GuestScheduleTemplate>().RemoveRange(existing);

        foreach (var templateId in dto.ScheduleTemplateIds)
        {
            _context.Set<GuestScheduleTemplate>().Add(new GuestScheduleTemplate
            {
                GuestId = guestId,
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
        var guest = await _context.Guests.FindAsync(id);
        if (guest == null) return NotFound();

        _context.Guests.Remove(guest);
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

        var guest = await _context.Guests.FindAsync(guestId);
        if (guest == null) return NotFound(new { message = "참석자를 찾을 수 없습니다." });
        
        if (guest.IsRegisteredUser)
            return BadRequest(new { message = "이미 회원입니다." });

        var existing = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == dto.LoginId);
        if (existing != null) 
            return BadRequest(new { field = "loginId", message = "이미 사용 중인 로그인 ID입니다." });

        var user = new User
        {
            LoginId = dto.LoginId,
            PasswordHash = _authService.HashPassword(dto.Password),
            Name = guest.GuestName,
            Phone = guest.Telephone,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        guest.UserId = user.Id;
        guest.IsRegisteredUser = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "회원으로 전환되었습니다.", userId = user.Id });
    }

    [HttpPut("guests/{guestId}/user")]
    public async Task<IActionResult> UpdateUserForGuest(int guestId, [FromBody] UpdateUserDto dto)
    {
        var guest = await _context.Guests.Include(g => g.User).FirstOrDefaultAsync(g => g.Id == guestId);
        if (guest?.User == null) return NotFound(new { message = "계정을 찾을 수 없습니다." });

        guest.User.Role = dto.Role;
        guest.User.UpdatedAt = DateTime.UtcNow;

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
    public async Task<IActionResult> GetAccessLink(int guestId)
    {
        var guest = await _context.Guests
            .Include(g => g.Convention)
            .FirstOrDefaultAsync(g => g.Id == guestId);
            
        if (guest == null) return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var link = $"{baseUrl}/guest/{guest.Convention.Id}/{guest.AccessToken}";

        return Ok(new { link, accessToken = guest.AccessToken });
    }

    [HttpPost("guests/{guestId}/send-sms")]
    public async Task<IActionResult> SendSMS(int guestId)
    {
        var guest = await _context.Guests
            .Include(g => g.Convention)
            .FirstOrDefaultAsync(g => g.Id == guestId);
            
        if (guest == null) return NotFound();

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
            .Include(gst => gst.Guest)
            .Select(gst => new
            {
                gst.Guest!.Id, gst.Guest.GuestName, gst.Guest.Telephone,
                gst.Guest.CorpPart, gst.Guest.Affiliation, gst.AssignedAt
            })
            .ToListAsync();

        return Ok(guests);
    }

    [HttpDelete("guests/{guestId}/schedules/{templateId}")]
    public async Task<IActionResult> RemoveGuestSchedule(int guestId, int templateId)
    {
        var assignment = await _context.Set<GuestScheduleTemplate>()
            .FirstOrDefaultAsync(gst => gst.GuestId == guestId && gst.ScheduleTemplateId == templateId);

        if (assignment == null) return NotFound();

        _context.Set<GuestScheduleTemplate>().Remove(assignment);
        await _context.SaveChangesAsync();
        return Ok();
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

    private string GenerateAccessToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    // ========== 게시판 관리 ==========
    
    [HttpGet("conventions/{conventionId}/notices")]
    public async Task<IActionResult> GetNotices(int conventionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Notices
            .Where(n => n.ConventionId == conventionId)
            .Include(n => n.Author)
            .Include(n => n.Attachments)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var notices = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Content,
                n.IsPinned,
                n.ViewCount,
                n.IsDeleted,
                AuthorName = n.Author.Name,
                n.CreatedAt,
                n.UpdatedAt,
                AttachmentCount = n.Attachments.Count
            })
            .ToListAsync();

        return Ok(new { items = notices, totalCount, page, pageSize });
    }

    [HttpPost("conventions/{conventionId}/notices")]
    public async Task<IActionResult> CreateNotice(int conventionId, [FromBody] CreateNoticeDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var notice = new Notice
        {
            ConventionId = conventionId,
            Title = dto.Title,
            Content = dto.Content,
            IsPinned = dto.IsPinned,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notices.Add(notice);
        await _context.SaveChangesAsync();

        return Ok(new { notice.Id, notice.Title, message = "공지사항이 생성되었습니다." });
    }

    [HttpPut("notices/{id}")]
    public async Task<IActionResult> UpdateNotice(int id, [FromBody] UpdateNoticeDto dto)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();

        notice.Title = dto.Title;
        notice.Content = dto.Content;
        notice.IsPinned = dto.IsPinned;
        notice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { notice.Id, notice.Title, message = "공지사항이 수정되었습니다." });
    }

    [HttpDelete("notices/{id}")]
    public async Task<IActionResult> DeleteNotice(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();

        notice.IsDeleted = true;
        notice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "공지사항이 삭제되었습니다." });
    }

    [HttpPost("notices/{id}/toggle-pin")]
    public async Task<IActionResult> TogglePin(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();

        notice.IsPinned = !notice.IsPinned;
        notice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { notice.IsPinned, message = notice.IsPinned ? "공지사항이 고정되었습니다." : "공지사항 고정이 해제되었습니다." });
    }
}

public class GuestDto
{
    public string GuestName { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string? CorpPart { get; set; }
    public string? ResidentNumber { get; set; }
    public string? Affiliation { get; set; }
    public string? Password { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}



public class CreateUserDto
{
    public string LoginId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Guest";
}

public class UpdateUserDto
{
    public string Role { get; set; } = "Guest";
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}


public class CreateNoticeDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}

public class UpdateNoticeDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
}
