namespace LocalRAG.Models.DTOs;

/// <summary>
/// 갤러리 생성 요청
/// </summary>
public class CreateGalleryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<GalleryImageDto>? Images { get; set; }
}

/// <summary>
/// 갤러리 수정 요청
/// </summary>
public class UpdateGalleryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<GalleryImageDto>? Images { get; set; }
}

/// <summary>
/// 갤러리 이미지 DTO
/// </summary>
public class GalleryImageDto
{
    public int? Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int OrderNum { get; set; }
}

/// <summary>
/// 갤러리 응답
/// </summary>
public class GalleryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<GalleryImageDto> Images { get; set; } = new();
}

/// <summary>
/// 갤러리 목록 아이템
/// </summary>
public class GalleryListItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ImageCount { get; set; }
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// 페이징된 갤러리 목록
/// </summary>
public class PagedGalleryResponse
{
    public List<GalleryListItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
