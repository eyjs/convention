using LocalRAG.DTOs.UserModels;
using LocalRAG.DTOs.ActionModels;
using Microsoft.AspNetCore.Http;

namespace LocalRAG.Interfaces;

/// <summary>
/// 사용자 프로필 및 참가자 관련 비즈니스 로직
/// </summary>
public interface IUserProfileService
{
    Task<object> GetMySchedulesAsync(int userId, int conventionId);
    Task<object> GetParticipantsAsync(int conventionId, string? search);
    Task<object?> GetParticipantDetailAsync(int id);
    Task<BulkAssignResult> BulkAssignAttributesAsync(BulkAssignAttributesDto dto);
    Task<List<UserWithAttributesDto>> GetParticipantsWithAttributesAsync(int conventionId);
    Task<(bool Success, string Message)> SubmitTravelInfoAsync(int userId, int conventionId, TravelInfoDto dto);
    Task<(bool Exists, object? Checklist)> GetMyChecklistAsync(int userId, int conventionId);
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateUserProfileDto dto);
    Task<(bool Success, string? ErrorMessage)> UpdateProfileFieldAsync(int userId, UpdateProfileFieldRequest request);
    Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<(bool Success, string? ErrorMessage, string? Url)> UploadProfilePhotoAsync(int userId, IFormFile file);
    Task<(bool Success, string? ErrorMessage, string? Url)> UploadPassportImageAsync(int userId, IFormFile file);
    Task<object> GetUserConventionsAsync(int userId);
    Task<object?> GetMyConventionInfoAsync(int userId, int conventionId);
}
