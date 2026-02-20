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
public class AdminStatsController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public AdminStatsController(ConventionDbContext context)
    {
        _context = context;
    }

    [HttpGet("conventions/{conventionId}/stats")]
    public async Task<IActionResult> GetStats(int conventionId)
    {
        var totalGuests = await _context.UserConventions
            .CountAsync(uc => uc.ConventionId == conventionId);
        var totalSchedules = await _context.ScheduleTemplates
            .CountAsync(st => st.ConventionId == conventionId);
        var scheduleAssignments = await _context.Set<GuestScheduleTemplate>()
            .Include(gst => gst.User)
                .ThenInclude(u => u.UserConventions)
            .CountAsync(gst => gst.User!.UserConventions.Any(uc => uc.ConventionId == conventionId));

        var recentGuests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .OrderByDescending(uc => uc.UserId)
            .Take(5)
            .Select(uc => new
            {
                Id = uc.UserId,
                Name = uc.User.Name,
                CorpPart = uc.User.CorpPart,
                Phone = uc.User.Phone
            })
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

        var smsHistory = await _context.SmsLogs
            .Where(l => l.ConventionId == conventionId)
            .OrderByDescending(l => l.SentAt)
            .Take(20)
            .Select(l => new
            {
                l.Id,
                l.ReceiverName,
                l.Message,
                SentAt = l.SentAt.AddHours(9).ToString("HH:mm"),
                Status = "success",
                StatusText = "발송성공"
            })
            .ToListAsync();

        return Ok(new
        {
            totalGuests,
            totalSchedules,
            scheduleAssignments,
            recentGuests,
            scheduleStats,
            attributeStats,
            smsHistory
        });
    }
}
