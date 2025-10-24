using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

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
            .OrderBy(f => f.MenuName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Feature>> GetEnabledFeaturesAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(f => f.ConventionId == conventionId && f.IsActive)
            .OrderBy(f => f.MenuName)
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
                     f.MenuUrl == featureName && 
                     f.IsActive,
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
