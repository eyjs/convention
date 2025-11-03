using LocalRAG.Data;
using LocalRAG.DTOs.ScheduleModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.User;

[ApiController]
[Route("api/user-schedules")]
public class UserScheduleController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public UserScheduleController(ConventionDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}/{conventionId}")]
    public async Task<IActionResult> GetSchedules(int userId, int conventionId)
    {
        try
        {
            var userInConvention = await _context.UserConventions
                .AnyAsync(uc => uc.UserId == userId && uc.ConventionId == conventionId);

            if (!userInConvention)
                return NotFound(new { message = "User not found in this convention" });

            var user = await _context.Users
                .Include(u => u.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return NotFound(new { message = "User not found" });

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

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddSchedule([FromBody] UserScheduleDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user is null) return NotFound("User not found.");

        var scheduleTemplate = await _context.ScheduleTemplates.FindAsync(dto.ScheduleTemplateId);
        if (scheduleTemplate is null) return NotFound("Schedule Template not found.");

        var existing = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.UserId == dto.UserId && gst.ScheduleTemplateId == dto.ScheduleTemplateId);

        if (existing is not null)
            return Conflict("Schedule template is already assigned to this user.");

        var userSchedule = new GuestScheduleTemplate
        {
            UserId = dto.UserId,
            ScheduleTemplateId = dto.ScheduleTemplateId
        };

        _context.GuestScheduleTemplates.Add(userSchedule);
        await _context.SaveChangesAsync();

        return Ok(userSchedule);
    }

    [HttpDelete("{userId}/{scheduleTemplateId}")]
    public async Task<IActionResult> RemoveSchedule(int userId, int scheduleTemplateId)
    {
        var userSchedule = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.UserId == userId && gst.ScheduleTemplateId == scheduleTemplateId);

        if (userSchedule is null) return NotFound();

        _context.GuestScheduleTemplates.Remove(userSchedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("templates/{userId}")]
    public async Task<IActionResult> GetAssignedTemplates(int userId)
    {
        var user = await _context.Users
            .Include(u => u.GuestScheduleTemplates)
            .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return NotFound();

        var templates = user.GuestScheduleTemplates
            .Select(gst => gst.ScheduleTemplate)
            .Where(st => st is not null)
            .ToList();

        return Ok(templates);
    }
}