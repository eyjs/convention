using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

public class HomeBannerService : IHomeBannerService
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeBannerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<HomeBanner>> GetActiveBannersAsync()
    {
        var banners = await _unitOfWork.HomeBanners.Query
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
        return banners;
    }

    public async Task<List<HomeBanner>> GetAllBannersAsync()
    {
        var banners = await _unitOfWork.HomeBanners.Query
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
        return banners;
    }

    public async Task<HomeBanner?> GetBannerByIdAsync(int id)
    {
        return await _unitOfWork.HomeBanners.GetByIdAsync(id);
    }

    public async Task<HomeBanner> CreateBannerAsync(HomeBanner banner)
    {
        var maxOrder = await _unitOfWork.HomeBanners.Query
            .MaxAsync(b => (int?)b.SortOrder) ?? 0;
        banner.SortOrder = maxOrder + 1;
        banner.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.HomeBanners.AddAsync(banner);
        await _unitOfWork.SaveChangesAsync();
        return banner;
    }

    public async Task<HomeBanner?> UpdateBannerAsync(int id, HomeBanner updated)
    {
        var banner = await _unitOfWork.HomeBanners.GetByIdAsync(id);
        if (banner == null) return null;

        banner.ImageUrl = updated.ImageUrl;
        banner.LinkUrl = updated.LinkUrl;
        banner.LinkLabel = updated.LinkLabel;
        banner.Title = updated.Title;
        banner.DetailImagesJson = updated.DetailImagesJson;
        banner.IsActive = updated.IsActive;

        _unitOfWork.HomeBanners.Update(banner);
        await _unitOfWork.SaveChangesAsync();
        return banner;
    }

    public async Task<bool> DeleteBannerAsync(int id)
    {
        var banner = await _unitOfWork.HomeBanners.GetByIdAsync(id);
        if (banner == null) return false;

        _unitOfWork.HomeBanners.Remove(banner);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderBannersAsync(List<int> ids)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            var banner = await _unitOfWork.HomeBanners.GetByIdAsync(ids[i]);
            if (banner != null)
            {
                banner.SortOrder = i;
                _unitOfWork.HomeBanners.Update(banner);
            }
        }
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
