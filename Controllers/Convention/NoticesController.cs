using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using LocalRAG.Repositories;

using LocalRAG.DTOs.NoticeModels;
using LocalRAG.Extensions;
using LocalRAG.Services.Convention;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Convention;

[ApiController]
[Route("api/notices")]
public class NoticesController : ControllerBase
{
    private readonly INoticeService _noticeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationSendService _notificationSendService;
    private readonly ILogger<NoticesController> _logger;

    public NoticesController(INoticeService noticeService, IUnitOfWork unitOfWork, NotificationSendService notificationSendService, ILogger<NoticesController> logger)
    {
        _noticeService = noticeService;
        _unitOfWork = unitOfWork;
        _notificationSendService = notificationSendService;
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
            var userId = User.GetUserId();
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
            var userId = User.GetUserId();
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
            var userId = User.GetUserId();
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
            var userId = User.GetUserId();
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
            var comments = await _unitOfWork.Comments.Query
                .IgnoreQueryFilters()
                .Where(c => c.NoticeId == noticeId)
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
            var userIdNullable = User.GetUserIdOrNull();
            if (userIdNullable == null)
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }
            var userId = userIdNullable.Value;

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
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

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var response = new CommentResponse
            {
                Id = comment.Id,
                NoticeId = comment.NoticeId,
                AuthorId = comment.AuthorId,
                AuthorName = authorName,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };

            // 관리자에게 댓글 알림 발송 (비동기, 실패해도 댓글 저장은 유지)
            _ = Task.Run(async () =>
            {
                try
                {
                    var notice = await _unitOfWork.Notices.GetByIdAsync(noticeId);
                    if (notice == null) return;

                    var conventionId = notice.ConventionId;
                    var adminIds = await _unitOfWork.UserConventions.Query
                        .Where(uc => uc.ConventionId == conventionId)
                        .Include(uc => uc.User)
                        .Where(uc => uc.User.Role == Roles.Admin && uc.UserId != authorId)
                        .Select(uc => uc.UserId)
                        .ToListAsync();

                    if (adminIds.Count == 0) return;

                    await _notificationSendService.SendAsync(conventionId, authorId, new SendNotificationRequest
                    {
                        Type = "NOTICE",
                        Title = $"새 댓글: {notice.Title}",
                        Body = $"{authorName}: {(request.Content.Length > 50 ? request.Content[..50] + "..." : request.Content)}",
                        ReferenceId = noticeId,
                        TargetScope = "INDIVIDUAL",
                        TargetUserIds = adminIds
                    });
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "댓글 알림 발송 실패 (댓글은 정상 저장됨)");
                }
            });

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
            var userIdNullable = User.GetUserIdOrNull();
            if (userIdNullable == null)
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }
            var userId = userIdNullable.Value;

            var comment = await _unitOfWork.Comments.Query
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound(new { message = "댓글을 찾을 수 없습니다." });

            // 작성자 본인만 수정 가능
            if (comment.AuthorId != userId)
                return Forbid("자신의 댓글만 수정할 수 있습니다.");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

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
            var userIdNullable = User.GetUserIdOrNull();
            if (userIdNullable == null)
            {
                return Unauthorized("사용자 정보를 확인할 수 없습니다.");
            }
            var userId = userIdNullable.Value;

            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);

            if (comment == null) return NotFound(new { message = "댓글을 찾을 수 없습니다." });

            // 작성자 본인만 삭제 가능
            if (comment.AuthorId != userId)
            {
                return Forbid("자신의 댓글만 삭제할 수 있습니다.");
            }

            comment.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "댓글 삭제 실패: {CommentId}", commentId);
            return StatusCode(500, new { message = "댓글 삭제에 실패했습니다." });
        }
    }
}
