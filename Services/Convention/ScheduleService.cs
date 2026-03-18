using LocalRAG.Data;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

/// <summary>
/// 일정 관리 서비스
/// Admin/User 컨트롤러의 일정 비즈니스 로직을 통합
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(ConventionDbContext context, ILogger<ScheduleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // Admin 일정 템플릿 관리
    // ============================================================

    public async Task<object> GetScheduleTemplatesAsync(int conventionId)
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

        return templates;
    }

    public async Task<ScheduleTemplate> CreateScheduleTemplateAsync(int conventionId, ScheduleTemplateDto dto)
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

        return template;
    }

    public async Task<ScheduleTemplate?> UpdateScheduleTemplateAsync(int id, ScheduleTemplateDto dto)
    {
        var template = await _context.ScheduleTemplates.FindAsync(id);
        if (template == null) return null;

        template.CourseName = dto.CourseName;
        template.Description = dto.Description;
        template.OrderNum = dto.OrderNum;

        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteScheduleTemplateAsync(int id)
    {
        var template = await _context.ScheduleTemplates
            .Include(st => st.ScheduleItems)
            .Include(st => st.GuestScheduleTemplates)
            .FirstOrDefaultAsync(st => st.Id == id);

        if (template == null) return false;

        if (template.GuestScheduleTemplates.Any())
            _context.Set<GuestScheduleTemplate>().RemoveRange(template.GuestScheduleTemplates);

        if (template.ScheduleItems.Any())
            _context.ScheduleItems.RemoveRange(template.ScheduleItems);

        _context.ScheduleTemplates.Remove(template);
        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // Admin 일정 항목 관리
    // ============================================================

    public async Task<ScheduleItem> CreateScheduleItemAsync(ScheduleItemDto dto)
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

        return item;
    }

    public async Task<ScheduleItem?> UpdateScheduleItemAsync(int id, ScheduleItemDto dto)
    {
        var item = await _context.ScheduleItems.FindAsync(id);
        if (item == null) return null;

        item.ScheduleDate = dto.ScheduleDate;
        item.StartTime = dto.StartTime;
        item.EndTime = dto.EndTime;
        item.Title = dto.Title;
        item.Location = dto.Location;
        item.Content = dto.Content;
        item.OrderNum = dto.OrderNum;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteScheduleItemAsync(int id)
    {
        var item = await _context.ScheduleItems.FindAsync(id);
        if (item == null) return false;

        _context.ScheduleItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(int Count, string Message)> BulkCreateScheduleItemsAsync(BulkScheduleItemsDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return (0, "복사할 일정이 없습니다.");

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

        return (items.Count, $"{items.Count}개 일정이 추가되었습니다.");
    }

    // ============================================================
    // Admin 게스트-일정 배정
    // ============================================================

    public async Task<object> GetTemplateGuestsAsync(int templateId)
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

        return guests;
    }

    public async Task<(bool Success, string? Error)> AssignSchedulesToGuestAsync(
        int conventionId, int guestId, AssignSchedulesDto dto)
    {
        var user = await _context.Users.FindAsync(guestId);
        if (user == null) return (false, "User not found");

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
        return (true, null);
    }

    public async Task<bool> RemoveGuestFromScheduleAsync(int userId, int templateId)
    {
        var assignment = await _context.Set<GuestScheduleTemplate>()
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == templateId);

        if (assignment == null) return false;

        _context.Set<GuestScheduleTemplate>().Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetAllSchedulesAsync(int conventionId)
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

        return items;
    }

    // ============================================================
    // User 일정 조회/관리
    // ============================================================

    public async Task<(List<ScheduleItemDto>? Result, string? Error, int StatusCode)> GetUserScheduleAsync(
        int userId, int conventionId)
    {
        try
        {
            var userInConvention = await _context.UserConventions
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userInConvention)
                return (null, "User not found in this convention", 404);

            var user = await _context.Users
                .Include(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return (null, "User not found", 404);

            var schedules = user.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate is not null && gst.ScheduleTemplate.ConventionId == conventionId)
                .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems.Select(si => new
                {
                    ScheduleItem = si,
                    TemplateId = gst.ScheduleTemplate.Id,
                    gst.ScheduleTemplate.CourseName
                }))
                .OrderBy(x => x.ScheduleItem.ScheduleDate)
                .ThenBy(x => x.ScheduleItem.StartTime)
                .ToList();

            var templateIds = schedules.Select(s => s.TemplateId).Distinct().ToList();
            var participantCounts = await _context.GuestScheduleTemplates
                .Where(gst => templateIds.Contains(gst.ScheduleTemplateId))
                .GroupBy(gst => gst.ScheduleTemplateId)
                .Select(g => new { TemplateId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TemplateId, x => x.Count);

            var result = schedules.Select(s => new ScheduleItemDto
            {
                Id = s.ScheduleItem.Id,
                ScheduleTemplateId = s.TemplateId,
                ScheduleDate = s.ScheduleItem.ScheduleDate,
                StartTime = s.ScheduleItem.StartTime,
                EndTime = s.ScheduleItem.EndTime,
                Title = s.ScheduleItem.Title,
                Content = s.ScheduleItem.Content,
                Location = s.ScheduleItem.Location,
                OrderNum = s.ScheduleItem.OrderNum,
                CourseName = s.CourseName,
                ParticipantCount = participantCounts.GetValueOrDefault(s.TemplateId, 0)
            }).ToList();

            return (result, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "일정 조회 중 오류 발생: userId={UserId}, conventionId={ConventionId}", userId, conventionId);
            return (null, "Internal server error", 500);
        }
    }

    public async Task<(GuestScheduleTemplate? Result, string? Error, int StatusCode)> AddUserScheduleAsync(
        UserScheduleDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user is null)
            return (null, "User not found.", 404);

        var scheduleTemplate = await _context.ScheduleTemplates.FindAsync(dto.ScheduleTemplateId);
        if (scheduleTemplate is null)
            return (null, "Schedule Template not found.", 404);

        var existing = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.UserId == dto.UserId && gst.ScheduleTemplateId == dto.ScheduleTemplateId);

        if (existing is not null)
            return (null, "Schedule template is already assigned to this user.", 409);

        var userSchedule = new GuestScheduleTemplate
        {
            UserId = dto.UserId,
            ScheduleTemplateId = dto.ScheduleTemplateId
        };

        _context.GuestScheduleTemplates.Add(userSchedule);
        await _context.SaveChangesAsync();

        return (userSchedule, null, 200);
    }

    public async Task<bool> RemoveUserScheduleAsync(int userId, int scheduleTemplateId)
    {
        var userSchedule = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == scheduleTemplateId);

        if (userSchedule is null) return false;

        _context.GuestScheduleTemplates.Remove(userSchedule);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(object? Result, bool Found)> GetAssignedTemplatesAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.GuestScheduleTemplates)
            .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return (null, false);

        var templates = user.GuestScheduleTemplates
            .Select(gst => gst.ScheduleTemplate)
            .Where(st => st is not null)
            .ToList();

        return (templates, true);
    }

    public async Task<(object? Result, string? Error, int StatusCode)> GetOptionToursAsync(
        int userId, int conventionId)
    {
        try
        {
            var userInConvention = await _context.UserConventions
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userInConvention)
                return (null, "User not found in this convention", 404);

            var optionToursData = await _context.UserOptionTours
                .Where(uot => uot.UserId == userId && uot.ConventionId == conventionId)
                .Include(uot => uot.OptionTour)
                .OrderBy(uot => uot.OptionTour!.Date)
                .ThenBy(uot => uot.OptionTour!.StartTime)
                .ToListAsync();

            var optionTours = optionToursData.Select(uot => new
            {
                id = uot.OptionTour!.Id,
                date = uot.OptionTour.Date.ToString("yyyy-MM-dd"),
                startTime = uot.OptionTour.StartTime,
                endTime = uot.OptionTour.EndTime,
                name = uot.OptionTour.Name,
                content = uot.OptionTour.Content,
                customOptionId = uot.OptionTour.CustomOptionId
            }).ToList();

            return (optionTours, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "옵션투어 조회 중 오류 발생: userId={UserId}, conventionId={ConventionId}", userId, conventionId);
            return (null, "Internal server error", 500);
        }
    }

    public async Task<(object? Result, string? Error, int StatusCode)> GetScheduleParticipantsAsync(
        int scheduleTemplateId, string? userRole)
    {
        try
        {
            var template = await _context.ScheduleTemplates
                .FirstOrDefaultAsync(st => st.Id == scheduleTemplateId);

            if (template is null)
                return (null, "Schedule template not found", 404);

            var isAdmin = userRole == "Admin";

            var participants = await _context.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplateId == scheduleTemplateId)
                .Include(gst => gst.User)
                .Select(gst => new
                {
                    id = gst.User.Id,
                    name = gst.User.Name,
                    organization = gst.User.CorpName ?? gst.User.Affiliation,
                    department = gst.User.CorpPart,
                    phone = isAdmin ? gst.User.Phone : null,
                    email = isAdmin ? gst.User.Email : null,
                    groupName = gst.User.Affiliation
                })
                .OrderBy(p => p.name)
                .ToListAsync();

            return (new
            {
                scheduleTemplateId,
                courseName = template.CourseName,
                totalCount = participants.Count,
                participants
            }, null, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참석자 목록 조회 중 오류 발생: scheduleTemplateId={TemplateId}", scheduleTemplateId);
            return (null, "Internal server error", 500);
        }
    }
}
