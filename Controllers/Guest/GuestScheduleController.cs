using LocalRAG.Data;
using LocalRAG.DTOs.ScheduleModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Guest;

[ApiController]
[Route("api/guest-schedules")]
public class GuestScheduleController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public GuestScheduleController(ConventionDbContext context)
    {
        _context = context;
    }

    [HttpGet("{guestId}/{conventionId}")]
    public async Task<IActionResult> GetSchedules(int guestId, int conventionId)
    {
        try
        {
            var guest = await _context.Guests
                .Include(g => g.GuestScheduleTemplates.Where(gst => gst.ScheduleTemplate!.ConventionId == conventionId))
                .ThenInclude(gst => gst.ScheduleTemplate)
                .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(g => g.Id == guestId && g.ConventionId == conventionId);

            if (guest is null) return NotFound(new { message = "Guest not found" });

            var schedules = guest.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate is not null)
                .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems.Select(si => new
                {
                    ScheduleItem = si,
                    TemplateId = gst.ScheduleTemplate.Id,
                    gst.ScheduleTemplate.CourseName
                }))
                .OrderBy(x => x.ScheduleItem.ScheduleDate)
                .ThenBy(x => x.ScheduleItem.StartTime)
                .ToList();

            // 각 템플릿별 참가자 수 조회
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
    public async Task<IActionResult> AddSchedule([FromBody] GuestScheduleDto dto)
    {
        var guest = await _context.Guests.FindAsync(dto.GuestId);
        if (guest is null) return NotFound("Guest not found.");

        var scheduleTemplate = await _context.ScheduleTemplates.FindAsync(dto.ScheduleTemplateId);
        if (scheduleTemplate is null) return NotFound("Schedule Template not found.");

        var existing = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.GuestId == dto.GuestId && gst.ScheduleTemplateId == dto.ScheduleTemplateId);

        if (existing is not null)
            return Conflict("Schedule template is already assigned to this guest.");

        var guestSchedule = new GuestScheduleTemplate
        {
            GuestId = dto.GuestId,
            ScheduleTemplateId = dto.ScheduleTemplateId
        };

        _context.GuestScheduleTemplates.Add(guestSchedule);
        await _context.SaveChangesAsync();

        return Ok(guestSchedule);
    }

    [HttpDelete("{guestId}/{scheduleTemplateId}")]
    public async Task<IActionResult> RemoveSchedule(int guestId, int scheduleTemplateId)
    {
        var guestSchedule = await _context.GuestScheduleTemplates
            .FirstOrDefaultAsync(gst => gst.GuestId == guestId && gst.ScheduleTemplateId == scheduleTemplateId);

        if (guestSchedule is null) return NotFound();

        _context.GuestScheduleTemplates.Remove(guestSchedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("templates/{guestId}")]
    public async Task<IActionResult> GetAssignedTemplates(int guestId)
    {
        var guest = await _context.Guests
            .Include(g => g.GuestScheduleTemplates)
            .ThenInclude(gst => gst.ScheduleTemplate)
            .FirstOrDefaultAsync(g => g.Id == guestId);

        if (guest is null) return NotFound();

        var templates = guest.GuestScheduleTemplates
            .Select(gst => gst.ScheduleTemplate)
            .Where(st => st is not null)
            .ToList();

        return Ok(templates);
    }
}