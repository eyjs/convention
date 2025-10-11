using LocalRAG.Data;
using LocalRAG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestScheduleController : ControllerBase
{
    private readonly ConventionDbContext _context;

    public GuestScheduleController(ConventionDbContext context)
    {
        _context = context;
    }

    // 참석자의 오늘/다음 일정
    [HttpGet("guests/{guestId}/today-next")]
    public async Task<IActionResult> GetTodayAndNextSchedule(int guestId)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        var guestSchedules = await _context.GuestScheduleTemplates
            .Where(gst => gst.GuestId == guestId)
            .Include(gst => gst.ScheduleTemplate)
                .ThenInclude(st => st.ScheduleItems)
            .ToListAsync();

        var allItems = guestSchedules
            .SelectMany(gst => gst.ScheduleTemplate.ScheduleItems.Select(si => new
            {
                si.Id,
                si.ScheduleDate,
                si.StartTime,
                si.Title,
                si.Location,
                si.Content,
                TemplateId = gst.ScheduleTemplateId,
                CourseName = gst.ScheduleTemplate.CourseName
            }))
            .OrderBy(si => si.ScheduleDate)
            .ToList();

        // 오늘 일정
        var todaySchedules = allItems
            .Where(si => si.ScheduleDate.Date == today)
            .ToList();

        // 현재 진행중인 일정
        var currentSchedule = allItems
            .Where(si => si.ScheduleDate <= now && si.ScheduleDate.AddHours(2) > now)
            .OrderByDescending(si => si.ScheduleDate)
            .FirstOrDefault();

        // 다음 일정 (현재 시간 이후)
        var nextSchedule = allItems
            .Where(si => si.ScheduleDate > now)
            .OrderBy(si => si.ScheduleDate)
            .FirstOrDefault();

        // 내일 일정
        var tomorrowSchedules = allItems
            .Where(si => si.ScheduleDate.Date == tomorrow)
            .ToList();

        return Ok(new
        {
            current = currentSchedule,
            next = nextSchedule,
            today = todaySchedules,
            tomorrow = tomorrowSchedules,
            totalSchedules = allItems.Count
        });
    }

    // 참석자의 전체 일정 (날짜별 그룹)
    [HttpGet("guests/{guestId}/all")]
    public async Task<IActionResult> GetAllSchedules(int guestId)
    {
        var guestSchedules = await _context.GuestScheduleTemplates
            .Where(gst => gst.GuestId == guestId)
            .Include(gst => gst.ScheduleTemplate)
                .ThenInclude(st => st.ScheduleItems)
            .ToListAsync();

        var allItems = guestSchedules
            .SelectMany(gst => gst.ScheduleTemplate.ScheduleItems.Select(si => new
            {
                si.Id,
                si.ScheduleDate,
                si.StartTime,
                si.Title,
                si.Location,
                si.Content,
                si.OrderNum,
                TemplateId = gst.ScheduleTemplateId,
                CourseName = gst.ScheduleTemplate.CourseName
            }))
            .OrderBy(si => si.ScheduleDate)
            .ToList();

        // 날짜별로 그룹핑
        var groupedByDate = allItems
            .GroupBy(si => si.ScheduleDate.Date)
            .Select(g => new
            {
                date = g.Key,
                schedules = g.OrderBy(s => s.ScheduleDate).ToList()
            })
            .OrderBy(g => g.date)
            .ToList();

        return Ok(new
        {
            total = allItems.Count,
            byDate = groupedByDate,
            allSchedules = allItems
        });
    }

    // 행사 전체 일정 (비로그인 사용자용)
    [HttpGet("conventions/{conventionId}/public")]
    public async Task<IActionResult> GetConventionSchedules(int conventionId)
    {
        var templates = await _context.ScheduleTemplates
            .Where(st => st.ConventionId == conventionId)
            .Include(st => st.ScheduleItems)
            .OrderBy(st => st.OrderNum)
            .ToListAsync();

        var allItems = templates
            .SelectMany(t => t.ScheduleItems.Select(si => new
            {
                si.Id,
                si.ScheduleDate,
                si.StartTime,
                si.Title,
                si.Location,
                si.Content,
                TemplateId = t.Id,
                CourseName = t.CourseName
            }))
            .OrderBy(si => si.ScheduleDate)
            .ToList();

        var groupedByDate = allItems
            .GroupBy(si => si.ScheduleDate.Date)
            .Select(g => new
            {
                date = g.Key,
                schedules = g.OrderBy(s => s.ScheduleDate).ToList()
            })
            .OrderBy(g => g.date)
            .ToList();

        return Ok(new
        {
            total = allItems.Count,
            byDate = groupedByDate
        });
    }
}
