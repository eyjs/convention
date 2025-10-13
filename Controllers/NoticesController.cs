using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Models.DTOs;
using System.Security.Claims;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoticesController : ControllerBase
{
    private readonly INoticeService _noticeService;
    private readonly ILogger<NoticesController> _logger;

    public NoticesController(INoticeService noticeService, ILogger<NoticesController> logger)
    {
        _noticeService = noticeService;
        _logger = logger;
    }

    /// <summary>
    /// 공지사항 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedNoticeResponse>> GetNotices(
        [FromQuery] int conventionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchType = null,
        [FromQuery] string? searchKeyword = null)
    {
        try
        {
            var result = await _noticeService.GetNoticesAsync(
                conventionId,
                page,
                pageSize,
                searchType,
                searchKeyword);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 목록 조회 실패");
            return StatusCode(500, new { message = "공지사항 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 공지사항 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NoticeResponse>> GetNotice(int id)
    {
        try
        {
            var notice = await _noticeService.GetNoticeAsync(id);
            return Ok(notice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 조회 실패: {Id}", id);
            return StatusCode(500, new { message = "공지사항을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 공지사항 생성 (관리자)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NoticeResponse>> CreateNotice(
        [FromQuery] int conventionId,
        [FromBody] CreateNoticeRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.CreateNoticeAsync(conventionId, request, userId);
            return CreatedAtAction(nameof(GetNotice), new { id = notice.Id }, notice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 생성 실패");
            return StatusCode(500, new { message = "공지사항 생성에 실패했습니다." });
        }
    }

    /// <summary>
    /// 공지사항 수정 (관리자 또는 작성자)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<NoticeResponse>> UpdateNotice(int id, [FromBody] UpdateNoticeRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.UpdateNoticeAsync(id, request, userId);
            return Ok(notice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 수정 실패: {Id}", id);
            return StatusCode(500, new { message = "공지사항 수정에 실패했습니다." });
        }
    }

    /// <summary>
    /// 공지사항 삭제 (소프트 삭제)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotice(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _noticeService.DeleteNoticeAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 삭제 실패: {Id}", id);
            return StatusCode(500, new { message = "공지사항 삭제에 실패했습니다." });
        }
    }

    /// <summary>
    /// 조회수 증가
    /// </summary>
    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncrementViewCount(int id)
    {
        try
        {
            await _noticeService.IncrementViewCountAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "조회수 증가 실패: {Id}", id);
            return StatusCode(500, new { message = "조회수 증가에 실패했습니다." });
        }
    }

    /// <summary>
    /// 공지사항 고정/해제 (관리자)
    /// </summary>
    [HttpPost("{id}/toggle-pin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NoticeResponse>> TogglePin(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.TogglePinAsync(id, userId);
            return Ok(notice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공지사항 고정 토글 실패: {Id}", id);
            return StatusCode(500, new { message = "공지사항 고정 상태 변경에 실패했습니다." });
        }
    }
}
