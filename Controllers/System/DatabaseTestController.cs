using LocalRAG.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.System;

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
        var guestCount = await _context.UserConventions.CountAsync(uc => uc.ConventionId == conventionId);
        var templateCount = await _context.ScheduleTemplates.CountAsync(st => st.ConventionId == conventionId);
        var itemCount = await _context.ScheduleItems.CountAsync();

        var guests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .Select(uc => new { Id = uc.UserId, GuestName = uc.User.Name, uc.ConventionId })
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
            var userConventions = await _context.UserConventions
                .Where(uc => uc.ConventionId == conventionId)
                .Include(uc => uc.User)
                    .ThenInclude(u => u.GuestAttributes)
                .Include(uc => uc.User)
                    .ThenInclude(u => u.GuestScheduleTemplates)
                        .ThenInclude(ust => ust.ScheduleTemplate)
                .ToListAsync();

            return Ok(new
            {
                count = userConventions.Count,
                data = userConventions.Select(uc => new
                {
                    Id = uc.UserId,
                    GuestName = uc.User.Name,
                    uc.ConventionId,
                    attributeCount = uc.User.GuestAttributes.Count,
                    scheduleCount = uc.User.GuestScheduleTemplates.Count
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
