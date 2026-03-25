using LocalRAG.Constants;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminStatsController : ControllerBase
{
    private readonly IAdminStatsService _statsService;

    public AdminStatsController(IAdminStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("conventions/{conventionId}/stats")]
    public async Task<IActionResult> GetStats(int conventionId)
    {
        var stats = await _statsService.GetConventionStatsAsync(conventionId);
        return Ok(stats);
    }

    [HttpGet("conventions/{conventionId}/stats/schedule-assigned-users")]
    public async Task<IActionResult> GetScheduleAssignedUsers(int conventionId)
    {
        var users = await _statsService.GetScheduleAssignedUsersAsync(conventionId);
        return Ok(users);
    }

    [HttpGet("conventions/{conventionId}/stats/schedule-course-users/{scheduleTemplateId}")]
    public async Task<IActionResult> GetScheduleCourseUsers(int conventionId, int scheduleTemplateId)
    {
        var users = await _statsService.GetScheduleCourseUsersAsync(conventionId, scheduleTemplateId);
        return Ok(users);
    }

    [HttpGet("conventions/{conventionId}/stats/attribute-users")]
    public async Task<IActionResult> GetAttributeUsers(int conventionId, [FromQuery] string key, [FromQuery] string value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            return BadRequest(new { message = "key와 value 파라미터는 필수입니다." });
        }

        var users = await _statsService.GetAttributeUsersAsync(conventionId, key, value);
        return Ok(users);
    }
}
