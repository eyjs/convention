using System.Text.Json;

namespace LocalRAG.Interfaces;

/// <summary>
/// 사용자 액션 서비스 인터페이스
/// UserActionController의 비즈니스 로직을 담당
/// </summary>
public interface IUserActionService
{
    /// <summary>
    /// 마감기한이 있는 긴급 액션 조회
    /// </summary>
    Task<object> GetUrgentActionsAsync(int conventionId);

    /// <summary>
    /// 모든 활성 액션 조회 (필터링 지원)
    /// </summary>
    Task<object> GetAllActionsAsync(int conventionId, string? targetLocation, string? actionCategory, bool? isActive = null);

    /// <summary>
    /// 사용자의 액션 상태 목록 조회
    /// </summary>
    Task<object> GetUserActionStatusesAsync(int conventionId, int userId);

    /// <summary>
    /// 사용자 체크리스트 조회 (오케스트레이션 서비스 위임)
    /// </summary>
    Task<object> GetUserChecklistAsync(int conventionId, int userId);

    /// <summary>
    /// 액션 상세 정보 조회
    /// </summary>
    Task<object?> GetActionDetailAsync(int conventionId, int actionId);

    /// <summary>
    /// 액션 완료 처리
    /// </summary>
    Task<object?> CompleteActionAsync(int conventionId, int actionId, int userId, string? responseDataJson);

    /// <summary>
    /// 액션 완료/미완료 토글
    /// </summary>
    Task<object?> ToggleActionAsync(int conventionId, int actionId, int userId, bool isComplete);

    /// <summary>
    /// FormBuilder 타입 액션 데이터 제출
    /// </summary>
    /// <returns>성공 메시지 또는 null(액션 없음), 에러 메시지 문자열(유효성 실패)</returns>
    Task<(object? Result, string? Error, bool NotFound)> SubmitActionAsync(int conventionId, int actionId, int userId, JsonElement submissionData);

    /// <summary>
    /// 사용자 제출 데이터 조회
    /// </summary>
    Task<JsonElement?> GetUserSubmissionAsync(int actionId, int userId);

    /// <summary>
    /// 관리자용: 모든 제출 데이터 조회
    /// </summary>
    Task<(object? Result, string? Error, bool NotFound)> GetAllSubmissionsAsync(int conventionId, int actionId);

    /// <summary>
    /// 체크리스트 진행 상태 조회
    /// </summary>
    Task<object> GetChecklistStatusAsync(int conventionId, int userId);

    /// <summary>
    /// 메뉴 액션 조회
    /// </summary>
    Task<object> GetMenuActionsAsync(int conventionId);
}
