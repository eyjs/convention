using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// Schedule Repository 구현체
/// </summary>
public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
{
    public ScheduleRepository(ConventionDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 특정 행사의 모든 일정을 순서대로 조회합니다.
    /// 
    /// 정렬 기준:
    /// 1. ScheduleDate (날짜)
    /// 2. OrderNum (표시 순서)
    /// </summary>
    public async Task<IEnumerable<Schedule>> GetSchedulesByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ConventionId == conventionId)
            .OrderBy(s => s.ScheduleDate)
            .ThenBy(s => s.OrderNum)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Schedule>> GetSchedulesByDateAsync(
        DateTime date,
        int? conventionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(s => s.ScheduleDate.Date == date.Date);

        if (conventionId.HasValue)
        {
            query = query.Where(s => s.ConventionId == conventionId.Value);
        }

        return await query
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.OrderNum)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Schedule>> GetSchedulesByGroupAsync(
        string group,
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.Group == group && s.ConventionId == conventionId)
            .OrderBy(s => s.ScheduleDate)
            .ThenBy(s => s.OrderNum)
            .ToListAsync(cancellationToken);
    }
}
