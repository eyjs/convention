using LocalRAG.Entities;

namespace LocalRAG.Repositories;

/// <summary>
/// User Repository 인터페이스
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 전화번호로 사용자를 조회합니다.
    /// </summary>
    Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>
    /// LoginId로 사용자를 조회합니다.
    /// </summary>
    /// <param name="tracking">true면 EF가 변경 추적을 활성화 (수정 가능)</param>
    Task<User?> GetByLoginIdAsync(string loginId, CancellationToken cancellationToken = default, bool tracking = false);

    /// <summary>
    /// 이름으로 사용자를 검색합니다.
    /// </summary>
    Task<IEnumerable<User>> SearchByNameAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 행사의 사용자들을 검색합니다.
    /// </summary>
    Task<IEnumerable<User>> SearchUsersByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 행사에 참여하는 모든 사용자를 조회합니다.
    /// </summary>
    Task<IEnumerable<User>> GetUsersByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);
}
