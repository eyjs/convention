using LocalRAG.Data;
using LocalRAG.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Entities;
using LocalRAG.DTOs.ScheduleModels;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminScheduleController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public AdminScheduleController(ConventionDbContext context)
    {
        _context = context;
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

    [HttpPost("conventions/{conventionId}/guests/{guestId}/schedules")]
    public async Task<IActionResult> AssignSchedules(int conventionId, int guestId, [FromBody] AssignSchedulesDto dto)
    {
        var user = await _context.Users.FindAsync(guestId);
        if (user == null) return NotFound();

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
}
