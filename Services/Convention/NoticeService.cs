using Microsoft.EntityFrameworkCore;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using LocalRAG.DTOs.NoticeModels;
using LocalRAG.Repositories;

namespace LocalRAG.Services.Convention;

public class NoticeService : INoticeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NoticeService> _logger;

    public NoticeService(IUnitOfWork unitOfWork, ILogger<NoticeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedNoticeResponse> GetNoticesAsync(
        int conventionId,
        int page,
        int pageSize,
        string? searchType,
        string? searchKeyword)
    {
        var query = _unitOfWork.Notices.Query
            .Where(n => n.ConventionId == conventionId && !n.IsDeleted)
            .Include(n => n.Author)
            .Include(n => n.Attachments)
            .Include(n => n.NoticeCategory)
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

        // 정렬: 고정 공지사항 우선, DisplayOrder, 그 다음 최신순
        var notices = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenBy(n => n.DisplayOrder)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NoticeListItemResponse
            {
                Id = n.Id,
                Title = n.Title,
                IsPinned = n.IsPinned,
                DisplayOrder = n.DisplayOrder,
                ViewCount = n.ViewCount,
                AuthorName = n.Author.Name ?? "관리자",
                NoticeCategoryId = n.NoticeCategoryId,
                CategoryName = n.NoticeCategory != null ? n.NoticeCategory.Name : null,
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
        var notice = await _unitOfWork.Notices.Query
            .Include(n => n.Author)
            .Include(n => n.Attachments)
            .Include(n => n.NoticeCategory)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (notice == null)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 이전글/다음글 조회
        var prevNotice = await _unitOfWork.Notices.Query
            .Where(n => n.ConventionId == notice.ConventionId && !n.IsDeleted && n.CreatedAt < notice.CreatedAt)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoticeNavigationItem { Id = n.Id, Title = n.Title })
            .FirstOrDefaultAsync();

        var nextNotice = await _unitOfWork.Notices.Query
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
            NoticeCategoryId = notice.NoticeCategoryId,
            CategoryName = notice.NoticeCategory?.Name,
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
            NoticeCategoryId = request.NoticeCategoryId,
            AuthorId = authorId,
            ConventionId = conventionId,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Notices.AddAsync(notice);
        await _unitOfWork.SaveChangesAsync();

        // 첨부파일 연결
        if (request.AttachmentIds != null && request.AttachmentIds.Any())
        {
            var attachments = await _unitOfWork.FileAttachments.Query
                .Where(f => request.AttachmentIds.Contains(f.Id))
                .ToListAsync();

            foreach (var attachment in attachments)
            {
                attachment.NoticeId = notice.Id;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        return await GetNoticeAsync(notice.Id);
    }

    public async Task<NoticeResponse> UpdateNoticeAsync(int id, UpdateNoticeRequest request, int userId)
    {
        _logger.LogInformation("Updating notice {Id} by user {UserId}. Request: {@Request}", id, userId, request);
        var notice = await _unitOfWork.Notices.Query
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

        if (notice == null)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인 (작성자 또는 관리자만 수정 가능)
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (notice.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("수정 권한이 없습니다.");

        notice.Title = request.Title;
        notice.Content = request.Content;
        notice.IsPinned = request.IsPinned;
        notice.NoticeCategoryId = request.NoticeCategoryId;
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
            var newAttachments = await _unitOfWork.FileAttachments.Query
                .Where(f => request.AttachmentIds.Contains(f.Id))
                .ToListAsync();

            foreach (var attachment in newAttachments)
            {
                attachment.NoticeId = id;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetNoticeAsync(id);
    }

    public async Task DeleteNoticeAsync(int id, int userId)
    {
        var notice = await _unitOfWork.Notices.GetByIdAsync(id);

        if (notice == null || notice.IsDeleted)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (notice.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("삭제 권한이 없습니다.");

        notice.IsDeleted = true;
        notice.UpdatedAt = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var notice = await _unitOfWork.Notices.GetByIdAsync(id);
        if (notice != null && !notice.IsDeleted)
        {
            notice.ViewCount++;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<NoticeResponse> TogglePinAsync(int id, int userId)
    {
        var notice = await _unitOfWork.Notices.GetByIdAsync(id);

        if (notice == null || notice.IsDeleted)
            throw new KeyNotFoundException("공지사항을 찾을 수 없습니다.");

        // 권한 확인
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.Role != "Admin")
            throw new UnauthorizedAccessException("관리자만 고정할 수 있습니다.");

        notice.IsPinned = !notice.IsPinned;
        notice.UpdatedAt = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();

        return await GetNoticeAsync(id);
    }

    public async Task UpdateNoticeOrderAsync(List<NoticeOrderItem> orders, int userId)
    {
        // 권한 확인
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.Role != "Admin")
            throw new UnauthorizedAccessException("관리자만 순서를 변경할 수 있습니다.");

        // 각 공지사항의 DisplayOrder 업데이트
        foreach (var order in orders)
        {
            var notice = await _unitOfWork.Notices.GetByIdAsync(order.Id);
            if (notice != null && !notice.IsDeleted)
            {
                notice.DisplayOrder = order.DisplayOrder;
                notice.UpdatedAt = DateTime.Now;
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
