using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using LocalRAG.DTOs.GalleryModels;

namespace LocalRAG.Services.Convention;

public class GalleryService : IGalleryService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<GalleryService> _logger;

    public GalleryService(ConventionDbContext context, ILogger<GalleryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedGalleryResponse> GetGalleriesAsync(int conventionId, int page, int pageSize)
    {
        var query = _context.Set<Gallery>()
            .Where(g => g.ConventionId == conventionId && !g.IsDeleted)
            .Include(g => g.Author)
            .Include(g => g.Images)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var galleries = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GalleryListItemResponse
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                AuthorName = g.Author.Name ?? "관리자",
                CreatedAt = g.CreatedAt,
                ImageCount = g.Images.Count,
                ThumbnailUrl = g.Images.OrderBy(i => i.OrderNum).FirstOrDefault() != null 
                    ? g.Images.OrderBy(i => i.OrderNum).First().ImageUrl 
                    : null
            })
            .ToListAsync();

        return new PagedGalleryResponse
        {
            Items = galleries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GalleryResponse> GetGalleryAsync(int id)
    {
        var gallery = await _context.Set<Gallery>()
            .Include(g => g.Author)
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (gallery == null)
            throw new KeyNotFoundException("갤러리를 찾을 수 없습니다.");

        return new GalleryResponse
        {
            Id = gallery.Id,
            Title = gallery.Title,
            Description = gallery.Description,
            AuthorName = gallery.Author.Name ?? "관리자",
            CreatedAt = gallery.CreatedAt,
            Images = gallery.Images
                .OrderBy(i => i.OrderNum)
                .Select(i => new GalleryImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    OrderNum = i.OrderNum
                })
                .ToList()
        };
    }

    public async Task<GalleryResponse> CreateGalleryAsync(int conventionId, CreateGalleryRequest request, int authorId)
    {
        var gallery = new Gallery
        {
            Title = request.Title,
            Description = request.Description,
            AuthorId = authorId,
            ConventionId = conventionId,
            CreatedAt = DateTime.Now
        };

        _context.Set<Gallery>().Add(gallery);
        await _context.SaveChangesAsync();

        // 이미지 추가
        if (request.Images != null && request.Images.Any())
        {
            var images = request.Images.Select(img => new GalleryImage
            {
                GalleryId = gallery.Id,
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                OrderNum = img.OrderNum,
                UploadedAt = DateTime.Now
            }).ToList();

            _context.Set<GalleryImage>().AddRange(images);
            await _context.SaveChangesAsync();
        }

        return await GetGalleryAsync(gallery.Id);
    }

    public async Task<GalleryResponse> UpdateGalleryAsync(int id, UpdateGalleryRequest request, int userId)
    {
        var gallery = await _context.Set<Gallery>()
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (gallery == null)
            throw new KeyNotFoundException("갤러리를 찾을 수 없습니다.");

        // 권한 확인
        var user = await _context.Users.FindAsync(userId);
        if (gallery.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("수정 권한이 없습니다.");

        gallery.Title = request.Title;
        gallery.Description = request.Description;
        gallery.UpdatedAt = DateTime.Now;

        // 기존 이미지 삭제
        _context.Set<GalleryImage>().RemoveRange(gallery.Images);

        // 새 이미지 추가
        if (request.Images != null && request.Images.Any())
        {
            var images = request.Images.Select(img => new GalleryImage
            {
                GalleryId = gallery.Id,
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                OrderNum = img.OrderNum,
                UploadedAt = DateTime.Now
            }).ToList();

            _context.Set<GalleryImage>().AddRange(images);
        }

        await _context.SaveChangesAsync();
        return await GetGalleryAsync(id);
    }

    public async Task DeleteGalleryAsync(int id, int userId)
    {
        var gallery = await _context.Set<Gallery>().FindAsync(id);

        if (gallery == null || gallery.IsDeleted)
            throw new KeyNotFoundException("갤러리를 찾을 수 없습니다.");

        // 권한 확인
        var user = await _context.Users.FindAsync(userId);
        if (gallery.AuthorId != userId && user?.Role != "Admin")
            throw new UnauthorizedAccessException("삭제 권한이 없습니다.");

        gallery.IsDeleted = true;
        gallery.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }
}
