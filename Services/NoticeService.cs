using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Services;

public class NoticeService : INoticeService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<NoticeService> _logger;

    public NoticeService(ConventionDbContext context, ILogger<NoticeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedNoticeResponse> GetNoticesAsync(
        int conventionId,
        int page,
        int pageSize,
        string? searchType,
        string? searchKeyword)
    {
        var query = _context.Notices
            .Where(n => n.ConventionId == conventionId && !n.IsDeleted)
            .Include(n => n.Author)
            .Include(n => n.Attachments)
            .AsQueryable();

        // 검색 필터
        if (!string.IsNullOrEmpty(searchKeyword))
        {
            searchKeyword = searchKeyword.Trim();
            query = searchType?.ToLower() switch
            {
                "title" => query.Where(n => n.Title.Contains(searchKeyword)),
                "content" => query.Where(n => n.Content.Contains(searchKeyword)),
                "all" => query.Where(n => n.Title.Contains(searchKeyword) || n.Content.Contains(searchKeyword)),
                _ => query.Where(n => n.Title.Contains(searchKeyword))
            };
        }

        var totalCount = await query.CountAsync();

        // 정렬: 고정 공지사항 우선, 그 다음 최신순
        var notices = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NoticeListItemResponse
            {
                Id = n.Id,
                Title = n.Title,
                IsPinned = n.IsPinned,
                ViewCount = n.ViewCount,
                AuthorName = n.Author.Name ?? "관리자",
                CreatedAt = n.CreatedAt,
                HasAttachment = n.Attachments.Any()
            })
            .ToListAsync();

        return new PagedNoticeResponse
        {
            Items = notices,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<NoticeResponse> GetNoticeAsync(int id)
    {
        var notice = await _context.Notices
            .Include(n => n.Author)
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (notice == null)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 이전글/다음글 조회
        var prevNotice = await _context.Notices
            .Where(n => n.ConventionId == notice.ConventionId && !n.IsDeleted && n.CreatedAt < notice.CreatedAt)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoticeNavigationItem { Id = n.Id, Title = n.Title })
            .FirstOrDefaultAsync();

        var nextNotice = await _context.Notices
            .Where(n => n.ConventionId == notice.ConventionId && !n.IsDeleted && n.CreatedAt > notice.CreatedAt)
            .OrderBy(n => n.CreatedAt)
            .Select(n => new NoticeNavigationItem { Id = n.Id, Title = n.Title })
            .FirstOrDefaultAsync();

        return new NoticeResponse
        {
            Id = notice.Id,
            Title = notice.Title,
            Content = notice.Content,
            IsPinned = notice.IsPinned,
            ViewCount = notice.ViewCount,
            AuthorName = notice.Author.Name ?? "관리자",
            AuthorId = notice.AuthorId,
            CreatedAt = notice.CreatedAt,
            UpdatedAt = notice.UpdatedAt,
            HasAttachment = notice.Attachments.Any(),
            Attachments = notice.Attachments.Select(a => new FileAttachmentResponse
            {
                Id = a.Id,
                Url = $"/api/files/{a.SavedName}",
                OriginalName = a.OriginalName,
                Size = a.Size,
                UploadedAt = a.UploadedAt
            }).ToList(),
            Navigation = new NoticeNavigationResponse
            {
                Prev = prevNotice,
                Next = nextNotice
            }
        };
    }

    public async Task<NoticeResponse> CreateNoticeAsync(int conventionId, CreateNoticeRequest request, int authorId)
    {
        var notice = new Notice
        {
            Title = request.Title,
            Content = request.Content,
            IsPinned = request.IsPinned,
            AuthorId = authorId,
            ConventionId = conventionId,
            CreatedAt = DateTime.Now
        };

        _context.Notices.Add(notice);
        await _context.SaveChangesAsync();

        // 첨부파일 연결
        if (request.AttachmentIds != null && request.AttachmentIds.Any())
        {
            var attachments = await _context.FileAttachments
                .Where(f => request.AttachmentIds.Contains(f.Id))
                .ToListAsync();

            foreach (var attachment in attachments)
            {
                attachment.NoticeId = notice.Id;
            }

            await _context.SaveChangesAsync();
        }

        return await GetNoticeAsync(notice.Id);
    }

    public async Task<NoticeResponse> UpdateNoticeAsync(int id, UpdateNoticeRequest request, int userId)
    {
        var notice = await _context.Notices
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (notice == null)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인 (작성자 또는 관리자만 수정 가능)
        var user = await _context.Users.FindAsync(userId);
        if (notice.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("수정 권한이 없습니다.");

        notice.Title = request.Title;
        notice.Content = request.Content;
        notice.IsPinned = request.IsPinned;
        notice.UpdatedAt = DateTime.Now;

        // 첨부파일 업데이트
        if (request.AttachmentIds != null)
        {
            // 기존 첨부파일 연결 해제
            foreach (var attachment in notice.Attachments.ToList())
            {
                attachment.NoticeId = null;
            }

            // 새 첨부파일 연결
            var newAttachments = await _context.FileAttachments
                .Where(f => request.AttachmentIds.Contains(f.Id))
                .ToListAsync();

            foreach (var attachment in newAttachments)
            {
                attachment.NoticeId = id;
            }
        }

        await _context.SaveChangesAsync();
        return await GetNoticeAsync(id);
    }

    public async Task DeleteNoticeAsync(int id, int userId)
    {
        var notice = await _context.Notices.FindAsync(id);

        if (notice == null || notice.IsDeleted)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인
        var user = await _context.Users.FindAsync(userId);
        if (notice.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("삭제 권한이 없습니다.");

        notice.IsDeleted = true;
        notice.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice != null && !notice.IsDeleted)
        {
            notice.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<NoticeResponse> TogglePinAsync(int id, int userId)
    {
        var notice = await _context.Notices.FindAsync(id);

        if (notice == null || notice.IsDeleted)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인
        var user = await _context.Users.FindAsync(userId);
        if (user?.Role != "Admin")
            throw new UnauthorizedAccessException("관리자만 고정할 수 있습니다.");

        notice.IsPinned = !notice.IsPinned;
        notice.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return await GetNoticeAsync(id);
    }
}
