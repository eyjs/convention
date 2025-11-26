namespace LocalRAG.DTOs.NoticeModels;

/// <summary>
/// 공지사항 생성 요청 DTO
/// </summary>
public class CreateNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
    public int? NoticeCategoryId { get; set; }
    public List<int>? AttachmentIds { get; set; }
}

/// <summary>
/// 공지사항 수정 요청 DTO
/// </summary>
public class UpdateNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
    public int? NoticeCategoryId { get; set; }
    public List<int>? AttachmentIds { get; set; }
}

/// <summary>
/// 공지사항 응답 DTO
/// </summary>
public class NoticeResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int? NoticeCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasAttachment { get; set; }
    public List<FileAttachmentResponse>? Attachments { get; set; }
    public NoticeNavigationResponse? Navigation { get; set; }
}

/// <summary>
/// 공지사항 목록 아이템 DTO
/// </summary>
public class NoticeListItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int? NoticeCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasAttachment { get; set; }
}

/// <summary>
/// 공지사항 네비게이션 (이전글/다음글)
/// </summary>
public class NoticeNavigationResponse
{
    public NoticeNavigationItem? Prev { get; set; }
    public NoticeNavigationItem? Next { get; set; }
}

public class NoticeNavigationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// 페이징된 공지사항 목록 응답
/// </summary>
public class PagedNoticeResponse
{
    public List<NoticeListItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// 파일 첨부 응답 DTO
/// </summary>
public class FileAttachmentResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// 파일 업로드 응답 DTO
/// </summary>
public class FileUploadResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// 공지사항 순서 업데이트 요청 DTO
/// </summary>
public class UpdateNoticeOrderRequest
{
    public List<NoticeOrderItem> Orders { get; set; } = new();
}

public class NoticeOrderItem
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}
