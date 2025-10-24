using LocalRAG.DTOs.NoticeModels;


namespace LocalRAG.Interfaces;

public interface INoticeService
{
    Task<PagedNoticeResponse> GetNoticesAsync(int conventionId, int page, int pageSize, string? searchType, string? searchKeyword);
    Task<NoticeResponse> GetNoticeAsync(int id);
    Task<NoticeResponse> CreateNoticeAsync(int conventionId, CreateNoticeRequest request, int authorId);
    Task<NoticeResponse> UpdateNoticeAsync(int id, UpdateNoticeRequest request, int userId);
    Task DeleteNoticeAsync(int id, int userId);
    Task IncrementViewCountAsync(int id);
    Task<NoticeResponse> TogglePinAsync(int id, int userId);
}
