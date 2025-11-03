using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// User Repository 구현체
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ConventionDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 전화번호로 사용자를 조회합니다.
    /// </summary>
    public async Task<User?> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);
    }

    /// <summary>
    /// LoginId로 사용자를 조회합니다.
    /// </summary>
    public async Task<User?> GetByLoginIdAsync(
        string loginId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.LoginId == loginId, cancellationToken);
    }

    /// <summary>
    /// 이름으로 사용자를 검색합니다.
    /// </summary>
    public async Task<IEnumerable<User>> SearchByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.Name.Contains(keyword))
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 이름으로 사용자를 검색합니다 (인터페이스 구현).
    /// </summary>
    public async Task<IEnumerable<User>> SearchUsersByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.Name.Contains(name))
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 특정 행사에 참여하는 모든 사용자를 조회합니다.
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.UserConventions.Any(uc => uc.ConventionId == conventionId))
            .Include(u => u.GuestAttributes)
            .Include(u => u.GuestScheduleTemplates)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }
}
