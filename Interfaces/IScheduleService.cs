using LocalRAG.DTOs.ScheduleModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 일정 관리 서비스 인터페이스
/// Admin/User 컨트롤러에서 공통으로 사용하는 일정 비즈니스 로직
/// </summary>
public interface IScheduleService
{
    // === Admin 일정 템플릿 관리 ===

    Task<object> GetScheduleTemplatesAsync(int conventionId);
    Task<ScheduleTemplate> CreateScheduleTemplateAsync(int conventionId, ScheduleTemplateDto dto);
    Task<ScheduleTemplate?> UpdateScheduleTemplateAsync(int id, ScheduleTemplateDto dto);
    Task<bool> DeleteScheduleTemplateAsync(int id);

    // === Admin 일정 항목 관리 ===

    Task<ScheduleItem> CreateScheduleItemAsync(ScheduleItemDto dto);
    Task<ScheduleItem?> UpdateScheduleItemAsync(int id, ScheduleItemDto dto);
    Task<bool> DeleteScheduleItemAsync(int id);
    Task<(int Count, string Message)> BulkCreateScheduleItemsAsync(BulkScheduleItemsDto dto);

    // === Admin 게스트-일정 배정 ===

    Task<object> GetTemplateGuestsAsync(int templateId);
    Task<(bool Success, string? Error)> AssignSchedulesToGuestAsync(int conventionId, int guestId, AssignSchedulesDto dto);
    Task<(bool Success, string? Error)> AssignOptionToursToGuestAsync(int conventionId, int guestId, List<int> optionTourIds);
    Task<bool> RemoveGuestFromScheduleAsync(int userId, int templateId);
    Task<object> GetAllSchedulesAsync(int conventionId);

    // === User 일정 조회/관리 ===

    Task<(List<ScheduleItemDto>? Result, string? Error, int StatusCode)> GetUserScheduleAsync(int userId, int conventionId);
    Task<(GuestScheduleTemplate? Result, string? Error, int StatusCode)> AddUserScheduleAsync(UserScheduleDto dto);
    Task<bool> RemoveUserScheduleAsync(int userId, int scheduleTemplateId);
    Task<(object? Result, bool Found)> GetAssignedTemplatesAsync(int userId);
    Task<(object? Result, string? Error, int StatusCode)> GetOptionToursAsync(int userId, int conventionId);
    Task<(object? Result, string? Error, int StatusCode)> GetScheduleParticipantsAsync(int scheduleTemplateId, string? userRole);

    // === Admin 옵션투어 관리 ===

    Task<object> GetOptionToursByConventionAsync(int conventionId);
    Task<object> CreateOptionTourAsync(int conventionId, OptionTourAdminDto dto);
    Task<object?> UpdateOptionTourAsync(int id, OptionTourAdminDto dto);
    Task<bool> DeleteOptionTourAsync(int id);
    Task<object> GetOptionTourParticipantsAsync(int optionTourId);
    Task<(bool Success, object Result, int StatusCode)> AddParticipantsToOptionTourAsync(int optionTourId, List<int> userIds);
    Task<bool> RemoveParticipantFromOptionTourAsync(int optionTourId, int userId);
}
