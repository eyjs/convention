namespace LocalRAG.Interfaces;

public interface ICompanionService
{
    /// <summary>
    /// 행사 내 특정 사용자의 동반자 목록 조회
    /// </summary>
    Task<object> GetCompanionsAsync(int conventionId, int userId);

    /// <summary>
    /// 동반자 관계 추가
    /// </summary>
    Task<(bool Success, object Result, int StatusCode)> AddCompanionAsync(
        int conventionId, int userId, int companionUserId, string relationType);

    /// <summary>
    /// 동반자 관계 삭제
    /// </summary>
    Task<(bool Success, object Result, int StatusCode)> RemoveCompanionAsync(int relationId);

    /// <summary>
    /// 행사의 모든 동반자 관계 조회 (관리자용)
    /// </summary>
    Task<object> GetAllCompanionRelationsAsync(int conventionId);
}
