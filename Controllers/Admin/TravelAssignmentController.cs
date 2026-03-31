using LocalRAG.Constants;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin/conventions/{conventionId}/travel-assignments")]
[Authorize(Roles = Roles.Admin)]
public class TravelAssignmentController : ControllerBase
{
    private readonly ITravelAssignmentService _service;

    public TravelAssignmentController(ITravelAssignmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// 전체 참석자의 여행 배정 목록
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int conventionId)
    {
        var result = await _service.GetAllAssignmentsAsync(conventionId);
        return Ok(result);
    }

    /// <summary>
    /// 개별 유저의 특정 일자 정보 수정
    /// </summary>
    [HttpPut("users/{userId}/day")]
    public async Task<IActionResult> UpdateDay(int conventionId, int userId, [FromBody] TravelDayUpdateRequest request)
    {
        request.UserId = userId;
        await _service.UpdateTravelDayAsync(conventionId, userId, request);
        return Ok(new { success = true });
    }

    /// <summary>
    /// 일괄 업데이트
    /// </summary>
    [HttpPut("bulk")]
    public async Task<IActionResult> BulkUpdate(int conventionId, [FromBody] List<TravelDayUpdateRequest> assignments)
    {
        var result = await _service.BulkUpdateAsync(conventionId, assignments);
        return Ok(result);
    }
}
