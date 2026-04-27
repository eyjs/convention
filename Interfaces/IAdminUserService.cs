using LocalRAG.DTOs.AdminModels;

namespace LocalRAG.Interfaces;

public interface IAdminUserService
{
    Task<object> GetGuestsAsync(int conventionId);
    Task<object?> GetGuestDetailAsync(int guestId, int? conventionId = null);
    Task<(bool Success, object Result, int StatusCode)> CreateGuestAsync(int conventionId, UserDto dto);
    Task<(bool Success, object Result, int StatusCode)> UpdateGuestAsync(int id, UserDto dto);
    Task<(bool Found, int StatusCode)> DeleteGuestAsync(int id);
    Task<(bool Success, object Result, int StatusCode)> ConvertToUserAsync(int guestId, CreateUserDto dto);
    Task<(bool Success, object Result, int StatusCode)> UpdateUserRoleAsync(int guestId, UpdateUserDto dto);
    Task<object> GetUsersAsync(string? searchTerm, string? role, int page, int pageSize);
    Task<(bool Found, object? Result, int StatusCode)> ToggleUserStatusAsync(int id, UpdateUserStatusDto dto);
    Task<(bool Success, object Result, int StatusCode)> UpdateUserRoleDirectAsync(int id, UpdateUserRoleDto dto);
    Task<(bool Success, object Result, int StatusCode)> ResetPasswordAsync(int userId, ResetPasswordDto dto);
    Task<(bool Found, object? Result)> GetAccessLinkAsync(int guestId, int? conventionId, string baseUrl);
    Task<(bool Success, object Result, int StatusCode)> LinkExistingUsersAsync(int conventionId, List<int> userIds, string? groupName);
    Task<(bool Success, object Result, int StatusCode)> TogglePassportVerificationAsync(int userId, bool verified);
}
