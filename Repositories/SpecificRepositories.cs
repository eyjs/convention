using LocalRAG.Data;
using LocalRAG.Models.Convention;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// Companion Repository 구현체
/// </summary>
public class CompanionRepository : Repository<Companion>, ICompanionRepository
{
    public CompanionRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Companion>> GetCompanionsByGuestIdAsync(
        int guestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.GuestId == guestId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// GuestSchedule Repository 구현체
/// 
/// Many-to-Many 관계 처리:
/// - Guest ↔ Schedule 간의 다대다 관계를 중간 테이블로 관리
/// - Composite Key (GuestId + ScheduleId)를 사용
/// </summary>
public class GuestScheduleRepository : Repository<GuestSchedule>, IGuestScheduleRepository
{
    public GuestScheduleRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GuestSchedule>> GetSchedulesByGuestIdAsync(
        int guestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(gs => gs.Schedule)  // 일정 정보 포함
            .Where(gs => gs.GuestId == guestId)
            .OrderBy(gs => gs.Schedule.ScheduleDate)
            .ThenBy(gs => gs.Schedule.OrderNum)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GuestSchedule>> GetGuestsByScheduleIdAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(gs => gs.Guest)  // 참석자 정보 포함
            .Where(gs => gs.ScheduleId == scheduleId)
            .OrderBy(gs => gs.Guest.GuestName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 참석자를 일정에 등록합니다.
    /// 
    /// 중복 체크:
    /// - 이미 등록된 경우 기존 데이터 반환
    /// - 새로 등록하는 경우 추가 후 반환
    /// </summary>
    public async Task<GuestSchedule> AssignGuestToScheduleAsync(
        int guestId,
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        // 중복 체크
        var existing = await _dbSet
            .FirstOrDefaultAsync(
                gs => gs.GuestId == guestId && gs.ScheduleId == scheduleId,
                cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        // 새로 등록
        var guestSchedule = new GuestSchedule
        {
            GuestId = guestId,
            ScheduleId = scheduleId
        };

        await _dbSet.AddAsync(guestSchedule, cancellationToken);
        return guestSchedule;
    }

    /// <summary>
    /// 참석자를 일정에서 제거합니다.
    /// 
    /// Composite Key 삭제:
    /// - Find로 복합키를 사용하여 조회
    /// - 존재하면 삭제
    /// </summary>
    public async Task<bool> RemoveGuestFromScheduleAsync(
        int guestId,
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        var guestSchedule = await _dbSet
            .FirstOrDefaultAsync(
                gs => gs.GuestId == guestId && gs.ScheduleId == scheduleId,
                cancellationToken);

        if (guestSchedule == null)
        {
            return false;
        }

        _dbSet.Remove(guestSchedule);
        return true;
    }
}

/// <summary>
/// Feature Repository 구현체
/// </summary>
public class FeatureRepository : Repository<Feature>, IFeatureRepository
{
    public FeatureRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Feature>> GetFeaturesByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(f => f.ConventionId == conventionId)
            .OrderBy(f => f.FeatureName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Feature>> GetEnabledFeaturesAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(f => f.ConventionId == conventionId && f.IsEnabled == "Y")
            .OrderBy(f => f.FeatureName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsFeatureEnabledAsync(
        int conventionId,
        string featureName,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                f => f.ConventionId == conventionId && 
                     f.FeatureName == featureName && 
                     f.IsEnabled == "Y",
                cancellationToken);
    }
}

/// <summary>
/// Menu Repository 구현체
/// </summary>
public class MenuRepository : Repository<Menu>, IMenuRepository
{
    public MenuRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Menu>> GetMenusByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.ConventionId == conventionId)
            .OrderBy(m => m.OrderNum)
            .ToListAsync(cancellationToken);
    }

    public async Task<Menu?> GetMenuWithSectionsAsync(
        int menuId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Sections.OrderBy(s => s.OrderNum))
            .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken);
    }
}

/// <summary>
/// Section Repository 구현체
/// </summary>
public class SectionRepository : Repository<Section>, ISectionRepository
{
    public SectionRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Section>> GetSectionsByMenuIdAsync(
        int menuId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.MenuId == menuId)
            .OrderBy(s => s.OrderNum)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Owner Repository 구현체
/// </summary>
public class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Owner>> GetOwnersByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.ConventionId == conventionId)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// VectorStore Repository 구현체
/// 
/// 참고: VectorStore는 Guid를 PK로 사용하므로 
/// GetByIdAsync를 오버라이드해야 할 수 있습니다.
/// </summary>
public class VectorStoreRepository : Repository<VectorStore>, IVectorStoreRepository
{
    public VectorStoreRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VectorStore>> GetVectorsByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(v => v.ConventionId == conventionId)
            .OrderByDescending(v => v.RegDtm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VectorStore>> GetVectorsBySourceAsync(
        string sourceTable,
        string sourceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(v => v.SourceTable == sourceTable && v.SourceId == sourceId)
            .OrderByDescending(v => v.RegDtm)
            .ToListAsync(cancellationToken);
    }
}
