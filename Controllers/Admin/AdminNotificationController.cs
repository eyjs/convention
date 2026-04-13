using LocalRAG.Constants;
using LocalRAG.Extensions;
using LocalRAG.Services.Convention;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminNotificationController : ControllerBase
{
    private readonly NotificationSendService _service;

    public AdminNotificationController(NotificationSendService service) => _service = service;

    [HttpGet("conventions/{conventionId:int}/notifications")]
    public async Task<IActionResult> GetHistory(int conventionId)
        => Ok(await _service.GetHistoryAsync(conventionId));

    [HttpPost("conventions/{conventionId:int}/notifications")]
    public async Task<IActionResult> Send(int conventionId, [FromBody] SendNotificationRequest req)
    {
        var userId = User.GetUserId();
        var result = await _service.SendAsync(conventionId, userId, req);
        return Ok(result);
    }

    [HttpGet("notifications/{id:int}/stats")]
    public async Task<IActionResult> GetStats(int id)
    {
        var stats = await _service.GetStatsAsync(id);
        return stats == null ? NotFound() : Ok(stats);
    }
}
