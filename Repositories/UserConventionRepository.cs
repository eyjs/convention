using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.Utilities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// UserConvention Repository 구현체
/// User와 Convention 간의 다대다 관계를 관리합니다.
/// </summary>
public class UserConventionRepository : Repository<UserConvention>, IUserConventionRepository
{
    public UserConventionRepository(ConventionDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 특정 행사의 모든 참석자를 조회합니다.
    /// </summary>
    public async Task<IEnumerable<UserConvention>> GetByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Where(uc => uc.ConventionId == conventionId)
            .OrderBy(uc => uc.User.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 특정 사용자의 모든 행사 참여 정보를 조회합니다.
    /// </summary>
    public async Task<IEnumerable<UserConvention>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(uc => uc.Convention)
            .Where(uc => uc.UserId == userId)
            .OrderByDescending(uc => uc.Convention.StartDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 특정 사용자의 특정 행사 참여 정보를 조회합니다.
    /// </summary>
    public async Task<UserConvention?> GetByUserAndConventionAsync(
        int userId,
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Include(uc => uc.Convention)
            .FirstOrDefaultAsync(
                uc => uc.UserId == userId && uc.ConventionId == conventionId,
                cancellationToken);
    }

    /// <summary>
    /// AccessToken으로 참여 정보를 조회합니다.
    /// 비회원 사용자 인증에 사용됩니다.
    /// </summary>
    public async Task<UserConvention?> GetByAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Include(uc => uc.Convention)
            .FirstOrDefaultAsync(
                uc => uc.AccessToken == accessToken,
                cancellationToken);
    }

    /// <summary>
    /// 특정 그룹의 참석자들을 조회합니다.
    /// 일정 배정 등에 사용됩니다.
    /// </summary>
    public async Task<IEnumerable<UserConvention>> GetByGroupNameAsync(
        int conventionId,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Where(uc => uc.ConventionId == conventionId && uc.GroupName == groupName)
            .OrderBy(uc => uc.User.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserConvention?> GetUserConventionWithUserAsync(
        int conventionId,
        string userName,
        string userPhone,
        CancellationToken cancellationToken = default)
    {
        // 전화번호는 정규화하여 비교해야 하므로 메모리에서 필터링
        var normalizedInputPhone = PhoneNumberFormatter.Normalize(userPhone);

        // Convention과 이름으로 먼저 필터링 (DB 쿼리)
        var candidates = await _dbSet
            .AsNoTracking()
            .Include(uc => uc.User)
            .Where(uc => uc.ConventionId == conventionId &&
                         uc.User.Name == userName)
            .ToListAsync(cancellationToken);

        // 전화번호 정규화하여 메모리에서 매칭
        return candidates.FirstOrDefault(uc =>
            PhoneNumberFormatter.Normalize(uc.User.Phone) == normalizedInputPhone);
    }
}
