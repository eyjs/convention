using LocalRAG.DTOs.NoticeModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Constants;
using LocalRAG.Extensions;

namespace LocalRAG.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notices")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminNoticesController : ControllerBase
    {
        private readonly INoticeService _noticeService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminNoticesController(INoticeService noticeService, IUnitOfWork unitOfWork)
        {
            _noticeService = noticeService;
            _unitOfWork = unitOfWork;
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
            var userId = User.GetUserId();
            var notice = await _noticeService.CreateNoticeAsync(conventionId, request, userId);
            return CreatedAtAction(nameof(GetNotice), new { id = notice.Id }, notice);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoticeResponse>> UpdateNotice(int id, [FromBody] UpdateNoticeRequest request)
        {
            var userId = User.GetUserId();
            var notice = await _noticeService.UpdateNoticeAsync(id, request, userId);
            return Ok(notice);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var userId = User.GetUserId();
            await _noticeService.DeleteNoticeAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/toggle-pin")]
        public async Task<ActionResult<NoticeResponse>> TogglePin(int id)
        {
            var userId = User.GetUserId();
            var notice = await _noticeService.TogglePinAsync(id, userId);
            return Ok(notice);
        }

        [HttpPut("update-order")]
        public async Task<IActionResult> UpdateNoticeOrder([FromBody] UpdateNoticeOrderRequest request)
        {
            var userId = User.GetUserId();
            await _noticeService.UpdateNoticeOrderAsync(request.Orders, userId);
            return NoContent();
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> HardDeleteComment(int commentId)
        {
            var comment = await _unitOfWork.Comments.Query
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return NotFound();

            _unitOfWork.Comments.Remove(comment);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
