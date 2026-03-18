using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.User;

[ApiController]
[Route("api/user-schedules")]
public class UserScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public UserScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("{userId}/{conventionId}")]
    public async Task<IActionResult> GetSchedules(int userId, int conventionId)
    {
        var (result, error, statusCode) = await _scheduleService.GetUserScheduleAsync(userId, conventionId);

        return statusCode switch
        {
            200 => Ok(result),
            404 => NotFound(new { message = error }),
            _ => StatusCode(statusCode, new { message = error })
        };
    }

    [HttpPost]
    public async Task<IActionResult> AddSchedule([FromBody] UserScheduleDto dto)
    {
        var (result, error, statusCode) = await _scheduleService.AddUserScheduleAsync(dto);

        return statusCode switch
        {
            200 => Ok(result),
            404 => NotFound(error),
            409 => Conflict(error),
            _ => StatusCode(statusCode, new { message = error })
        };
    }

    [HttpDelete("{userId}/{scheduleTemplateId}")]
    public async Task<IActionResult> RemoveSchedule(int userId, int scheduleTemplateId)
    {
        var removed = await _scheduleService.RemoveUserScheduleAsync(userId, scheduleTemplateId);
        if (!removed) return NotFound();
        return NoContent();
    }

    [HttpGet("templates/{userId}")]
    public async Task<IActionResult> GetAssignedTemplates(int userId)
    {
        var (result, found) = await _scheduleService.GetAssignedTemplatesAsync(userId);
        if (!found) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// 사용자의 옵션투어 조회
    /// </summary>
    [HttpGet("{userId}/{conventionId}/option-tours")]
    public async Task<IActionResult> GetOptionTours(int userId, int conventionId)
    {
        var (result, error, statusCode) = await _scheduleService.GetOptionToursAsync(userId, conventionId);

        return statusCode switch
        {
            200 => Ok(result),
            404 => NotFound(new { message = error }),
            _ => StatusCode(statusCode, new { message = error })
        };
    }

    /// <summary>
    /// 특정 일정 템플릿에 할당된 참석자 목록 조회
    /// 민감정보(연락처, 이메일)는 Admin 권한 사용자만 조회 가능
    /// </summary>
    [HttpGet("participants/{scheduleTemplateId}")]
    public async Task<IActionResult> GetScheduleParticipants(int scheduleTemplateId)
    {
        var userRole = User.FindFirst("role")?.Value
            ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        var (result, error, statusCode) = await _scheduleService.GetScheduleParticipantsAsync(scheduleTemplateId, userRole);

        return statusCode switch
        {
            200 => Ok(result),
            404 => NotFound(new { message = error }),
            _ => StatusCode(statusCode, new { message = error })
        };
    }
}
