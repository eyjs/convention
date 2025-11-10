using LocalRAG.DTOs.ActionModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 액션 오케스트레이터 서비스 인터페이스
/// 다양한 BehaviorType의 액션들을 통합하여 일관된 체크리스트를 제공
/// </summary>
public interface IActionOrchestrationService
{
    /// <summary>
    /// 특정 행사의 모든 액션을 사용자 상태와 함께 조회
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>통합된 액션 목록</returns>
    Task<IEnumerable<OrchestratedActionDto>> GetUserActionsAsync(int conventionId, int userId);

    /// <summary>
    /// 특정 액션의 상세 정보 조회
    /// </summary>
    /// <param name="actionId">액션 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>통합된 액션 정보</returns>
    Task<OrchestratedActionDto?> GetUserActionAsync(int actionId, int userId);
}
