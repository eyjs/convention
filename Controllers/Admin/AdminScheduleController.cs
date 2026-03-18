using LocalRAG.Constants;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public AdminScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("conventions/{conventionId}/schedule-templates")]
    public async Task<IActionResult> GetScheduleTemplates(int conventionId)
    {
        var templates = await _scheduleService.GetScheduleTemplatesAsync(conventionId);
        return Ok(templates);
    }

    [HttpPost("conventions/{conventionId}/schedule-templates")]
    public async Task<IActionResult> CreateScheduleTemplate(int conventionId, [FromBody] ScheduleTemplateDto dto)
    {
        var template = await _scheduleService.CreateScheduleTemplateAsync(conventionId, dto);
        return Ok(template);
    }

    [HttpPut("schedule-templates/{id}")]
    public async Task<IActionResult> UpdateScheduleTemplate(int id, [FromBody] ScheduleTemplateDto dto)
    {
        var template = await _scheduleService.UpdateScheduleTemplateAsync(id, dto);
        if (template == null) return NotFound();
        return Ok(template);
    }

    [HttpDelete("schedule-templates/{id}")]
    public async Task<IActionResult> DeleteScheduleTemplate(int id)
    {
        var deleted = await _scheduleService.DeleteScheduleTemplateAsync(id);
        if (!deleted) return NotFound();
        return Ok();
    }

    [HttpPost("schedule-items")]
    public async Task<IActionResult> CreateScheduleItem([FromBody] ScheduleItemDto dto)
    {
        var item = await _scheduleService.CreateScheduleItemAsync(dto);
        return Ok(item);
    }

    [HttpPut("schedule-items/{id}")]
    public async Task<IActionResult> UpdateScheduleItem(int id, [FromBody] ScheduleItemDto dto)
    {
        var item = await _scheduleService.UpdateScheduleItemAsync(id, dto);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("schedule-items/{id}")]
    public async Task<IActionResult> DeleteScheduleItem(int id)
    {
        var deleted = await _scheduleService.DeleteScheduleItemAsync(id);
        if (!deleted) return NotFound();
        return Ok();
    }

    [HttpPost("schedule-items/bulk")]
    public async Task<IActionResult> CreateScheduleItemsBulk([FromBody] BulkScheduleItemsDto dto)
    {
        var (count, message) = await _scheduleService.BulkCreateScheduleItemsAsync(dto);
        if (count == 0)
            return BadRequest(new { message });
        return Ok(new { message, count });
    }

    [HttpGet("schedule-templates/{templateId}/guests")]
    public async Task<IActionResult> GetTemplateGuests(int templateId)
    {
        var guests = await _scheduleService.GetTemplateGuestsAsync(templateId);
        return Ok(guests);
    }

    [HttpPost("conventions/{conventionId}/guests/{guestId}/schedules")]
    public async Task<IActionResult> AssignSchedules(int conventionId, int guestId, [FromBody] AssignSchedulesDto dto)
    {
        var (success, error) = await _scheduleService.AssignSchedulesToGuestAsync(conventionId, guestId, dto);
        if (!success) return NotFound();
        return Ok(new { message = "일정이 배정되었습니다." });
    }

    [HttpDelete("guests/{guestId}/schedules/{templateId}")]
    public async Task<IActionResult> RemoveGuestSchedule(int userId, int templateId)
    {
        var removed = await _scheduleService.RemoveGuestFromScheduleAsync(userId, templateId);
        if (!removed) return NotFound();
        return Ok();
    }

    [HttpGet("conventions/{conventionId}/schedules")]
    public async Task<IActionResult> GetAllSchedules(int conventionId)
    {
        var items = await _scheduleService.GetAllSchedulesAsync(conventionId);
        return Ok(items);
    }
}
