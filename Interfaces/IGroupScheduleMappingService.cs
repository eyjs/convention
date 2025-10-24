using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

/// <summary>
/// 그룹-일정 매핑 서비스 인터페이스
/// 특정 그룹의 모든 참석자에게 일정 템플릿을 일괄 배정
/// </summary>
public interface IGroupScheduleMappingService
{
    /// <summary>
    /// 특정 그룹의 모든 참석자에게 여러 일정(ConventionAction)을 배정
    /// </summary>
    /// <param name="request">그룹-일정 매핑 요청</param>
    /// <returns>매핑 결과</returns>
    Task<GroupScheduleMappingResult> MapGroupToSchedulesAsync(GroupScheduleMappingRequest request);

    /// <summary>
    /// 행사의 모든 그룹 목록 조회
    /// </summary>
    /// <param name="conventionId">행사 ID</param>
    /// <returns>그룹명 목록 (중복 제거)</returns>
    Task<List<string>> GetGroupsInConventionAsync(int conventionId);
}
