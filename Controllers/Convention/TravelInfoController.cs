using LocalRAG.Extensions;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/conventions/{conventionId}")]
[Authorize]
public class TravelInfoController : ControllerBase
{
    private readonly ITravelAssignmentService _service;

    public TravelInfoController(ITravelAssignmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// 사용자 본인의 여행 배정 정보 (호차/호텔/방번호/룸메이트)
    /// </summary>
    [HttpGet("my-travel-info")]
    public async Task<IActionResult> GetMyTravelInfo(int conventionId)
    {
        var userId = User.GetUserId();
        var result = await _service.GetMyTravelInfoAsync(conventionId, userId);

        if (result == null)
            return Ok(new { days = Array.Empty<object>() });

        return Ok(result);
    }
}
