using LocalRAG.DTOs.SeatingModels;

namespace LocalRAG.Interfaces;

public interface ISeatingLayoutService
{
    Task<List<SeatingLayoutListItemDto>> GetByConventionAsync(int conventionId);
    Task<SeatingLayoutDto?> GetByIdAsync(int id);
    Task<SeatingLayoutDto> CreateAsync(int conventionId, CreateSeatingLayoutRequest request);
    Task<SeatingLayoutDto?> UpdateAsync(int id, UpdateSeatingLayoutRequest request);
    Task<bool> DeleteAsync(int id);
    Task<SeatingLayoutDto?> SetBackgroundAsync(int id, string url);
    Task<SeatingLayoutDto?> DuplicateAsync(int id);
    Task<List<SeatingLayoutListItemDto>> GetMyLayoutsAsync(int conventionId, int userId);
    Task<(byte[] bytes, string filename)?> DownloadMembersExcelAsync(int layoutId);
    Task<object> UploadMembersExcelAsync(int layoutId, Stream stream);
}
