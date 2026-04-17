using LocalRAG.Entities;

namespace LocalRAG.Interfaces;

public interface IHomeBannerService
{
    Task<List<HomeBanner>> GetActiveBannersAsync();
    Task<List<HomeBanner>> GetAllBannersAsync();
    Task<HomeBanner?> GetBannerByIdAsync(int id);
    Task<HomeBanner> CreateBannerAsync(HomeBanner banner);
    Task<HomeBanner?> UpdateBannerAsync(int id, HomeBanner banner);
    Task<bool> DeleteBannerAsync(int id);
    Task<bool> ReorderBannersAsync(List<int> ids);
}
