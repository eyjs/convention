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

    [HttpPost("conventions/{conventionId}/guests/{guestId}/option-tours")]
    public async Task<IActionResult> AssignOptionTours(int conventionId, int guestId, [FromBody] AssignOptionToursDto dto)
    {
        var (success, error) = await _scheduleService.AssignOptionToursToGuestAsync(conventionId, guestId, dto.OptionTourIds);
        if (!success) return NotFound(new { message = error });
        return Ok(new { message = "옵션투어가 배정되었습니다." });
    }

    [HttpDelete("guests/{guestId}/schedules/{templateId}")]
    public async Task<IActionResult> RemoveGuestSchedule(int guestId, int templateId)
    {
        var removed = await _scheduleService.RemoveGuestFromScheduleAsync(guestId, templateId);
        if (!removed) return NotFound();
        return Ok();
    }

    [HttpGet("conventions/{conventionId}/schedules")]
    public async Task<IActionResult> GetAllSchedules(int conventionId)
    {
        var items = await _scheduleService.GetAllSchedulesAsync(conventionId);
        return Ok(items);
    }

    // === 옵션투어 관리 ===

    [HttpGet("conventions/{conventionId}/option-tours")]
    public async Task<IActionResult> GetOptionTours(int conventionId)
    {
        var optionTours = await _scheduleService.GetOptionToursByConventionAsync(conventionId);
        return Ok(optionTours);
    }

    [HttpPost("conventions/{conventionId}/option-tours")]
    public async Task<IActionResult> CreateOptionTour(int conventionId, [FromBody] OptionTourAdminDto dto)
    {
        var optionTour = await _scheduleService.CreateOptionTourAsync(conventionId, dto);
        return Ok(optionTour);
    }

    [HttpPut("option-tours/{id}")]
    public async Task<IActionResult> UpdateOptionTour(int id, [FromBody] OptionTourAdminDto dto)
    {
        var optionTour = await _scheduleService.UpdateOptionTourAsync(id, dto);
        if (optionTour == null) return NotFound();
        return Ok(optionTour);
    }

    [HttpDelete("option-tours/{id}")]
    public async Task<IActionResult> DeleteOptionTour(int id)
    {
        var deleted = await _scheduleService.DeleteOptionTourAsync(id);
        if (!deleted) return NotFound();
        return Ok();
    }

    [HttpGet("option-tours/{id}/participants")]
    public async Task<IActionResult> GetOptionTourParticipants(int id)
    {
        var participants = await _scheduleService.GetOptionTourParticipantsAsync(id);
        return Ok(participants);
    }

    [HttpPost("option-tours/{id}/participants")]
    public async Task<IActionResult> AddOptionTourParticipants(int id, [FromBody] AddParticipantsDto dto)
    {
        var (success, result, statusCode) = await _scheduleService.AddParticipantsToOptionTourAsync(id, dto.UserIds);
        return StatusCode(statusCode, result);
    }

    [HttpDelete("option-tours/{id}/participants/{userId}")]
    public async Task<IActionResult> RemoveOptionTourParticipant(int id, int userId)
    {
        var removed = await _scheduleService.RemoveParticipantFromOptionTourAsync(id, userId);
        if (!removed) return NotFound();
        return Ok();
    }
}
