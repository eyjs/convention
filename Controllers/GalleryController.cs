using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Models.DTOs;
using System.Security.Claims;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(IGalleryService galleryService, ILogger<GalleryController> logger)
    {
        _galleryService = galleryService;
        _logger = logger;
    }

    /// <summary>
    /// 갤러리 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedGalleryResponse>> GetGalleries(
        [FromQuery] int conventionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        try
        {
            var result = await _galleryService.GetGalleriesAsync(conventionId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "갤러리 목록 조회 실패");
            return StatusCode(500, new { message = "갤러리 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 갤러리 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GalleryResponse>> GetGallery(int id)
    {
        try
        {
            var gallery = await _galleryService.GetGalleryAsync(id);
            return Ok(gallery);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "갤러리 조회 실패: {Id}", id);
            return StatusCode(500, new { message = "갤러리를 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 갤러리 생성 (관리자)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GalleryResponse>> CreateGallery(
        [FromQuery] int conventionId,
        [FromBody] CreateGalleryRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var gallery = await _galleryService.CreateGalleryAsync(conventionId, request, userId);
            return CreatedAtAction(nameof(GetGallery), new { id = gallery.Id }, gallery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "갤러리 생성 실패");
            return StatusCode(500, new { message = "갤러리 생성에 실패했습니다." });
        }
    }

    /// <summary>
    /// 갤러리 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<GalleryResponse>> UpdateGallery(int id, [FromBody] UpdateGalleryRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var gallery = await _galleryService.UpdateGalleryAsync(id, request, userId);
            return Ok(gallery);
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
            _logger.LogError(ex, "갤러리 수정 실패: {Id}", id);
            return StatusCode(500, new { message = "갤러리 수정에 실패했습니다." });
        }
    }

    /// <summary>
    /// 갤러리 삭제
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteGallery(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _galleryService.DeleteGalleryAsync(id, userId);
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
            _logger.LogError(ex, "갤러리 삭제 실패: {Id}", id);
            return StatusCode(500, new { message = "갤러리 삭제에 실패했습니다." });
        }
    }
}
