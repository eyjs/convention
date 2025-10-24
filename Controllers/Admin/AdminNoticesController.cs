using LocalRAG.Data;
using LocalRAG.DTOs.NoticeModels;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LocalRAG.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notices")]
    [Authorize(Roles = "Admin")]
    public class AdminNoticesController : ControllerBase
    {
        private readonly INoticeService _noticeService;
        private readonly ConventionDbContext _context;

        public AdminNoticesController(INoticeService noticeService, ConventionDbContext context)
        {
            _noticeService = noticeService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedNoticeResponse>> GetNotices(
            [FromQuery] int conventionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchType = null,
            [FromQuery] string? searchKeyword = null)
        {
            var result = await _noticeService.GetNoticesAsync(conventionId, page, pageSize, searchType, searchKeyword);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoticeResponse>> GetNotice(int id)
        {
            var notice = await _noticeService.GetNoticeAsync(id);
            return Ok(notice);
        }

        [HttpPost]
        public async Task<ActionResult<NoticeResponse>> CreateNotice(
            [FromQuery] int conventionId,
            [FromBody] CreateNoticeRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.CreateNoticeAsync(conventionId, request, userId);
            return CreatedAtAction(nameof(GetNotice), new { id = notice.Id }, notice);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoticeResponse>> UpdateNotice(int id, [FromBody] UpdateNoticeRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.UpdateNoticeAsync(id, request, userId);
            return Ok(notice);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _noticeService.DeleteNoticeAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/toggle-pin")]
        public async Task<ActionResult<NoticeResponse>> TogglePin(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notice = await _noticeService.TogglePinAsync(id, userId);
            return Ok(notice);
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> HardDeleteComment(int commentId)
        {
            var comment = await _context.Comments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
