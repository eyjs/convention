using LocalRAG.DTOs.ConventionModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 행사(Convention) CRUD 비즈니스 로직 인터페이스
/// </summary>
public interface IConventionCrudService
{
    /// <summary>
    /// 행사 목록을 조회합니다 (soft-delete 필터 포함).
    /// </summary>
    Task<object> GetConventionsAsync(bool includeDeleted = false);

    /// <summary>
    /// 사용자가 참여한 행사 목록을 조회합니다.
    /// </summary>
    Task<object> GetMyConventionsAsync(int userId);

    /// <summary>
    /// 단일 행사를 상세 조회합니다 (통계 포함).
    /// </summary>
    Task<object?> GetConventionAsync(int id);

    /// <summary>
    /// 새로운 행사를 생성합니다.
    /// </summary>
    Task<object> CreateConventionAsync(CreateConventionRequest request, string userId);

    /// <summary>
    /// 행사 정보를 수정합니다.
    /// </summary>
    Task<object?> UpdateConventionAsync(int id, UpdateConventionRequest request);

    /// <summary>
    /// 행사를 소프트 삭제합니다.
    /// </summary>
    Task<bool> SoftDeleteConventionAsync(int id);

    /// <summary>
    /// 행사 완료 상태를 토글합니다.
    /// </summary>
    Task<object?> ToggleCompleteAsync(int id);

    /// <summary>
    /// 삭제된 행사를 복원합니다.
    /// </summary>
    Task<bool> RestoreConventionAsync(int id);

    /// <summary>
    /// 사용자의 탑승권 URL을 조회합니다.
    /// </summary>
    Task<string?> GetBoardingPassUrlAsync(int conventionId, int userId);
}
