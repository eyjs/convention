using LocalRAG.Data;
using LocalRAG.Models.Convention;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// Guest Repository 구현체
/// </summary>
public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Guest>> GetGuestsByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(g => g.ConventionId == conventionId)
            .OrderBy(g => g.GuestName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 참석자와 연관된 모든 정보를 포함하여 조회합니다.
    /// 
    /// 포함되는 데이터:
    /// - 참석자 속성 (GuestAttributes)
    /// - 동반자 (Companions)
    /// - 참여 일정 (GuestSchedules + Schedule 정보)
    /// </summary>
    public async Task<Guest?> GetGuestWithDetailsAsync(
        int guestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(g => g.Attributes)
            .Include(g => g.Companions)
            .Include(g => g.GuestSchedules)
                .ThenInclude(gs => gs.Schedule)
            .FirstOrDefaultAsync(g => g.Id == guestId, cancellationToken);
    }

    /// <summary>
    /// 이름으로 참석자를 검색합니다.
    /// 
    /// 검색 방식:
    /// - Contains: 부분 일치 검색 (LIKE '%name%')
    /// - conventionId가 있으면 특정 행사로 필터링
    /// </summary>
    public async Task<IEnumerable<Guest>> SearchGuestsByNameAsync(
        string guestName,
        int? conventionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(guestName))
        {
            query = query.Where(g => g.GuestName.Contains(guestName));
        }

        if (conventionId.HasValue)
        {
            query = query.Where(g => g.ConventionId == conventionId.Value);
        }

        return await query
            .OrderBy(g => g.GuestName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guest?> GetGuestByCorpHrIdAsync(
        string corpHrId,
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.CorpHrId == corpHrId && g.ConventionId == conventionId,
                cancellationToken);
    }
}
