using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Entities;
using LocalRAG.Interfaces;

namespace LocalRAG.Controllers.Convention;

[ApiController]
public class HomeBannerController : ControllerBase
{
    private readonly IHomeBannerService _bannerService;

    public HomeBannerController(IHomeBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    /// <summary>
    /// 활성 배너 목록 (사용자용)
    /// </summary>
    [HttpGet("api/home-banners")]
    [Authorize]
    public async Task<IActionResult> GetActiveBanners()
    {
        var banners = await _bannerService.GetActiveBannersAsync();
        return Ok(banners);
    }

    /// <summary>
    /// 배너 상세 (사용자용)
    /// </summary>
    [HttpGet("api/home-banners/{id}")]
    [Authorize]
    public async Task<IActionResult> GetBanner(int id)
    {
        var banner = await _bannerService.GetBannerByIdAsync(id);
        if (banner == null) return NotFound();
        return Ok(banner);
    }

    /// <summary>
    /// 전체 배너 목록 (관리자용)
    /// </summary>
    [HttpGet("api/admin/home-banners")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBanners()
    {
        var banners = await _bannerService.GetAllBannersAsync();
        return Ok(banners);
    }

    /// <summary>
    /// 배너 생성
    /// </summary>
    [HttpPost("api/admin/home-banners")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBanner([FromBody] HomeBannerDto dto)
    {
        var banner = new HomeBanner
        {
            ImageUrl = dto.ImageUrl,
            LinkUrl = dto.LinkUrl,
            LinkLabel = dto.LinkLabel,
            Title = dto.Title,
            DetailImagesJson = dto.DetailImagesJson,
            IsActive = dto.IsActive,
        };
        var created = await _bannerService.CreateBannerAsync(banner);
        return Ok(created);
    }

    /// <summary>
    /// 배너 수정
    /// </summary>
    [HttpPut("api/admin/home-banners/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBanner(int id, [FromBody] HomeBannerDto dto)
    {
        var updated = await _bannerService.UpdateBannerAsync(id, new HomeBanner
        {
            ImageUrl = dto.ImageUrl,
            LinkUrl = dto.LinkUrl,
            LinkLabel = dto.LinkLabel,
            Title = dto.Title,
            DetailImagesJson = dto.DetailImagesJson,
            IsActive = dto.IsActive,
        });
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// 배너 삭제
    /// </summary>
    [HttpDelete("api/admin/home-banners/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var deleted = await _bannerService.DeleteBannerAsync(id);
        if (!deleted) return NotFound();
        return Ok(new { success = true });
    }

    /// <summary>
    /// 배너 순서 변경
    /// </summary>
    [HttpPut("api/admin/home-banners/reorder")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReorderBanners([FromBody] List<int> ids)
    {
        await _bannerService.ReorderBannersAsync(ids);
        return Ok(new { success = true });
    }
}

public class HomeBannerDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkLabel { get; set; }
    public string? Title { get; set; }
    public string? DetailImagesJson { get; set; }
    public bool IsActive { get; set; } = true;
}
