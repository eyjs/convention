namespace LocalRAG.Interfaces;

public interface IChecklistService
{
    /// <summary>
    /// 체크리스트 상태 구축
    /// </summary>
    Task<object?> BuildChecklistStatusAsync(int userId, int conventionId);
}
