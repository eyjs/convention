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
    private readonly ILogger<TravelAssignmentController> _logger;

    public TravelAssignmentController(ITravelAssignmentService service, ILogger<TravelAssignmentController> logger)
    {
        _service = service;
        _logger = logger;
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

    /// <summary>
    /// 특정 날짜의 전체 배정 삭제
    /// </summary>
    [HttpDelete("dates/{date}")]
    public async Task<IActionResult> RemoveDate(int conventionId, string date)
    {
        var updated = await _service.RemoveDateAsync(conventionId, date);
        return Ok(new { success = true, updated });
    }

    /// <summary>
    /// 엑셀 업로드: 시트별 날짜, 행별 이름+전화번호+호차+호텔+방번호
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel(int conventionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "파일이 비어있습니다." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Excel 파일(.xlsx)만 업로드 가능합니다." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _service.UploadFromExcelAsync(conventionId, stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Travel assignment upload failed");
            return StatusCode(500, new { error = "업로드 중 오류가 발생했습니다." });
        }
    }
}
