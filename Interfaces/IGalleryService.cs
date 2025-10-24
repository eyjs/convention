using LocalRAG.DTOs.GalleryModels;

namespace LocalRAG.Interfaces;

public interface IGalleryService
{
    Task<PagedGalleryResponse> GetGalleriesAsync(int conventionId, int page, int pageSize);
    Task<GalleryResponse> GetGalleryAsync(int id);
    Task<GalleryResponse> CreateGalleryAsync(int conventionId, CreateGalleryRequest request, int authorId);
    Task<GalleryResponse> UpdateGalleryAsync(int id, UpdateGalleryRequest request, int userId);
    Task DeleteGalleryAsync(int id, int userId);
}
