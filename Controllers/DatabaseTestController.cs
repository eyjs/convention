using LocalRAG.Data;
using LocalRAG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/test")]
public class DatabaseTestController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public DatabaseTestController(ConventionDbContext context)
    {
        _context = context;
    }

    [HttpGet("check/{conventionId}")]
    public async Task<IActionResult> CheckData(int conventionId)
    {
        var guestCount = await _context.Guests.CountAsync(g => g.ConventionId == conventionId);
        var templateCount = await _context.ScheduleTemplates.CountAsync(st => st.ConventionId == conventionId);
        var itemCount = await _context.ScheduleItems.CountAsync();
        
        var guests = await _context.Guests
            .Where(g => g.ConventionId == conventionId)
            .Select(g => new { g.Id, g.GuestName, g.ConventionId })
            .ToListAsync();

        var templates = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .Select(st => new { st.Id, st.CourseName, st.ConventionId })
            .ToListAsync();

        return Ok(new
        {
            conventionId,
            guestCount,
            templateCount,
            itemCount,
            guests,
            templates,
            message = "데이터 확인 완료"
        });
    }

    [HttpGet("guests/{conventionId}")]
    public async Task<IActionResult> TestGuests(int conventionId)
    {
        try
        {
            var guests = await _context.Guests
                .Where(g => g.ConventionId == conventionId)
                .Include(g => g.GuestAttributes)
                .Include(g => g.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                .ToListAsync();

            return Ok(new
            {
                count = guests.Count,
                data = guests.Select(g => new
                {
                    g.Id,
                    g.GuestName,
                    g.ConventionId,
                    attributeCount = g.GuestAttributes.Count,
                    scheduleCount = g.GuestScheduleTemplates.Count
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
