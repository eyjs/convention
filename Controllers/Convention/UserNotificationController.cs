using LocalRAG.Extensions;
using LocalRAG.Services.Convention;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class UserNotificationController : ControllerBase
{
    private readonly NotificationSendService _service;

    public UserNotificationController(NotificationSendService service) => _service = service;

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int conventionId)
    {
        var userId = User.GetUserId();
        return Ok(await _service.GetMyNotificationsAsync(userId, conventionId));
    }

    [HttpGet("my/unread-count")]
    public async Task<IActionResult> UnreadCount([FromQuery] int conventionId)
    {
        var userId = User.GetUserId();
        return Ok(new { count = await _service.GetUnreadCountAsync(userId, conventionId) });
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.GetUserId();
        var ok = await _service.MarkReadAsync(id, userId);
        return ok ? Ok(new { message = "읽음 처리됨" }) : NotFound();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead([FromQuery] int conventionId)
    {
        var userId = User.GetUserId();
        var count = await _service.MarkAllReadAsync(userId, conventionId);
        return Ok(new { message = $"{count}건 읽음 처리됨", count });
    }
}
