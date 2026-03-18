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
}
