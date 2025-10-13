using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models;
using System.Security.Claims;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/guest")]
[Authorize]
public class GuestController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<GuestController> _logger;

    public GuestController(ConventionDbContext context, ILogger<GuestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 내 일정 조회 (로그인한 사용자)
    /// </summary>
    [HttpGet("my-schedules")]
    public async Task<IActionResult> GetMySchedules()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // 사용자의 Guest 정보 조회
            var guest = await _context.Guests
                .Include(g => g.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                        .ThenInclude(st => st!.ScheduleItems)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (guest == null)
                return NotFound(new { message = "참석자 정보를 찾을 수 없습니다." });

            // 할당된 모든 일정 항목 가져오기
            var schedules = guest.GuestScheduleTemplates
                .Where(gst => gst.ScheduleTemplate != null)
                .SelectMany(gst => gst.ScheduleTemplate!.ScheduleItems)
                .OrderBy(si => si.ScheduleDate)
                .ThenBy(si => si.StartTime)
                .Select(si => new
                {
                    si.Id,
                    ScheduleDate = si.ScheduleDate.ToString("yyyy-MM-dd"),
                    si.StartTime,
                    si.Title,
                    si.Location,
                    si.Content
                })
                .ToList();

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "일정 조회 실패");
            return StatusCode(500, new { message = "일정을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 목록 조회
    /// </summary>
    [HttpGet("participants")]
    public async Task<IActionResult> GetParticipants([FromQuery] int conventionId, [FromQuery] string? search = null)
    {
        try
        {
            var query = _context.Guests
                .Where(g => g.ConventionId == conventionId)
                .Include(g => g.GuestAttributes)
                .AsQueryable();

            // 검색
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(g => 
                    g.GuestName.Contains(search) || 
                    (g.CorpName != null && g.CorpName.Contains(search)) ||
                    (g.CorpPart != null && g.CorpPart.Contains(search)));
            }

            var participants = await query
                .OrderBy(g => g.GuestName)
                .Select(g => new
                {
                    g.Id,
                    g.GuestName,
                    g.CorpName,
                    g.CorpPart,
                    g.Telephone,
                    Attributes = g.GuestAttributes.ToDictionary(
                        ga => ga.AttributeKey,
                        ga => ga.AttributeValue
                    )
                })
                .ToListAsync();

            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 목록 조회 실패");
            return StatusCode(500, new { message = "참가자 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 참가자 상세 조회
    /// </summary>
    [HttpGet("participants/{id}")]
    public async Task<IActionResult> GetParticipant(int id)
    {
        try
        {
            var participant = await _context.Guests
                .Include(g => g.GuestAttributes)
                .Include(g => g.GuestScheduleTemplates)
                    .ThenInclude(gst => gst.ScheduleTemplate)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (participant == null)
                return NotFound(new { message = "참가자를 찾을 수 없습니다." });

            var result = new
            {
                participant.Id,
                participant.GuestName,
                participant.CorpName,
                participant.CorpPart,
                participant.Telephone,
                participant.Email,
                Attributes = participant.GuestAttributes.ToDictionary(
                    ga => ga.AttributeKey,
                    ga => ga.AttributeValue
                ),
                ScheduleTemplates = participant.GuestScheduleTemplates
                    .Where(gst => gst.ScheduleTemplate != null)
                    .Select(gst => new
                    {
                        gst.ScheduleTemplate!.Id,
                        gst.ScheduleTemplate.CourseName,
                        gst.ScheduleTemplate.Description
                    })
                    .ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "참가자 상세 조회 실패");
            return StatusCode(500, new { message = "참가자 정보를 불러오는데 실패했습니다." });
        }
    }
}
