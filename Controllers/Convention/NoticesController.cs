using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Entities;

using System.Security.Claims;
using LocalRAG.DTOs.NoticeModels;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/notices")]
public class NoticesController : ControllerBase
{
    private readonly INoticeService _noticeService;
    private readonly ConventionDbContext _context;
    private readonly ILogger<NoticesController> _logger;

    public NoticesController(INoticeService noticeService, ConventionDbContext context, ILogger<NoticesController> logger)
    {
        _noticeService = noticeService;
        _context = context;
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
    /// 조회수 증가 (세션 기반 - 중복 방지)
    /// </summary>
    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncrementViewCount(int id)
    {
        try
        {
            // 세션에서 조회한 공지 ID 목록 확인
            var viewedNotices = HttpContext.Session.GetString("ViewedNotices") ?? "";
            var viewedList = viewedNotices.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            // 이미 조회한 공지면 카운트 증가 안함
            if (viewedList.Contains(id.ToString()))
            {
                return Ok(new { message = "Already viewed", incremented = false });
            }
            
            await _noticeService.IncrementViewCountAsync(id);
            
            // 세션에 조회 기록 추가
            viewedList.Add(id.ToString());
            HttpContext.Session.SetString("ViewedNotices", string.Join(",", viewedList));
            
            return Ok(new { message = "View count incremented", incremented = true });
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

    // ========== 댓글 CRUD ==========

    /// <summary>
    /// 댓글 목록 조회
    /// </summary>
    [HttpGet("{noticeId}/comments")]
    public async Task<ActionResult<List<CommentResponse>>> GetComments(int noticeId)
    {
        try
        {
                    var isAdmin = User.IsInRole("Admin");
                    var query = _context.Comments.IgnoreQueryFilters().Where(c => c.NoticeId == noticeId);
            var comments = await query
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var responses = new List<CommentResponse>();

            foreach (var comment in comments)
            {
                string authorName = comment.Author?.Name ?? "익명";
                
                responses.Add(new CommentResponse
                {
                    Id = comment.Id,
                    NoticeId = comment.NoticeId,
                    AuthorId = comment.AuthorId,
                    AuthorName = authorName,
                    Content = comment.IsDeleted && !isAdmin ? "삭제된 메시지입니다." : comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    IsDeleted = comment.IsDeleted
                });
            }

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "댓글 목록 조회 실패: {NoticeId}", noticeId);
            return StatusCode(500, new { message = "댓글 목록을 불러오는데 실패했습니다." });
        }
    }

    /// <summary>
    /// 댓글 작성
    /// </summary>
    [HttpPost("{noticeId}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> CreateComment(int noticeId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("해당 사용자를 찾을 수 없습니다.");
            }

            int authorId = userId;
            string authorName = user.Name;
            _logger.LogInformation("Comment by User - UserId: {UserId}, Name: {AuthorName}", authorId, authorName);

            var comment = new Comment
            {
                NoticeId = noticeId,
                AuthorId = authorId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var response = new CommentResponse
            {
                Id = comment.Id,
                NoticeId = comment.NoticeId,
                AuthorId = comment.AuthorId,
                AuthorName = authorName,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };

            return CreatedAtAction(nameof(GetComments), new { noticeId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "댓글 작성 실패: {NoticeId}", noticeId);
            return StatusCode(500, new { message = "댓글 작성에 실패했습니다." });
        }
    }

    /// <summary>
    /// 댓글 수정
    /// </summary>
    [HttpPut("comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> UpdateComment(int commentId, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }

            var comment = await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound(new { message = "댓글을 찾을 수 없습니다." });

            // 작성자 본인만 수정 가능
            if (comment.AuthorId != userId)
                return Forbid("자신의 댓글만 수정할 수 있습니다.");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new CommentResponse
            {
                Id = comment.Id,
                NoticeId = comment.NoticeId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author?.Name ?? "익명",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "댓글 수정 실패: {CommentId}", commentId);
            return StatusCode(500, new { message = "댓글 수정에 실패했습니다." });
        }
    }

    /// <summary>
    /// 댓글 삭제
    /// </summary>
    [HttpDelete("comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("잘못된 사용자 정보입니다.");
            }

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null) return NotFound(new { message = "댓글을 찾을 수 없습니다." });

            // 작성자 본인만 삭제 가능
            if (comment.AuthorId != userId)
            {
                return Forbid("자신의 댓글만 삭제할 수 있습니다.");
            }

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "댓글 삭제 실패: {CommentId}", commentId);
            return StatusCode(500, new { message = "댓글 삭제에 실패했습니다." });
        }
    }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class CommentResponse
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
