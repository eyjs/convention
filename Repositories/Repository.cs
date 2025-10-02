using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LocalRAG.Repositories;

/// <summary>
/// 제네릭 Repository 구현체
/// IRepository 인터페이스를 구현하여 모든 엔티티에 대한 기본 데이터 액세스 로직을 제공합니다.
/// 
/// 작동 원리:
/// 1. DbContext를 DI(의존성 주입)로 받아서 데이터베이스에 접근
/// 2. DbSet<TEntity>를 통해 특정 엔티티 타입의 테이블에 접근
/// 3. EF Core의 Change Tracker를 활용하여 엔티티 상태 관리
/// </summary>
/// <typeparam name="TEntity">엔티티 타입</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ConventionDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(ConventionDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    // ============================================================
    // 조회 (Read) 메서드들
    // ============================================================

    /// <summary>
    /// ID로 엔티티를 조회합니다.
    /// AsNoTracking: 읽기 전용 조회로 성능 향상
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // FindAsync는 Primary Key로 빠르게 조회하며, 이미 Change Tracker에 있으면 DB 조회 생략
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// 조건에 맞는 단일 엔티티를 조회합니다.
    /// FirstOrDefaultAsync: 조건에 맞는 첫 번째 엔티티 반환, 없으면 null
    /// </summary>
    public virtual async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking() // 읽기 전용: Change Tracker에 등록하지 않아 메모리 효율 향상
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// 모든 엔티티를 조회합니다.
    /// 주의: 대량 데이터의 경우 페이징 처리 권장
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 조건에 맞는 모든 엔티티를 조회합니다.
    /// Where: LINQ 표현식을 SQL의 WHERE 절로 변환
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 페이징 처리된 결과를 조회합니다.
    /// 
    /// 작동 방식:
    /// 1. 전체 개수를 먼저 조회 (TotalCount)
    /// 2. Skip/Take로 해당 페이지 데이터만 조회
    /// 3. Tuple로 데이터와 총 개수를 함께 반환
    /// </summary>
    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

        // 기본 쿼리
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        // 조건이 있으면 필터 적용
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // 전체 개수 조회
        var totalCount = await query.CountAsync(cancellationToken);

        // 페이징 처리
        var items = await query
            .Skip((pageNumber - 1) * pageSize) // 이전 페이지들의 데이터를 건너뜀
            .Take(pageSize)                     // 현재 페이지 크기만큼만 가져옴
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// 조건에 맞는 엔티티가 존재하는지 확인합니다.
    /// AnyAsync: 데이터를 가져오지 않고 존재 여부만 확인 (성능 우수)
    /// </summary>
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// 조건에 맞는 엔티티의 개수를 반환합니다.
    /// </summary>
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // ============================================================
    // 생성 (Create) 메서드들
    // ============================================================

    /// <summary>
    /// 새로운 엔티티를 추가합니다.
    /// 
    /// 중요: 이 메서드는 Change Tracker에만 등록하고, 실제 DB에 저장하려면 SaveChangesAsync 호출 필요
    /// </summary>
    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // EntityState를 Added로 변경 (Change Tracker에 등록)
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// 여러 엔티티를 일괄 추가합니다.
    /// 대량 데이터 삽입 시 성능이 더 좋습니다.
    /// </summary>
    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    // ============================================================
    // 수정 (Update) 메서드들
    // ============================================================

    /// <summary>
    /// 엔티티를 수정합니다.
    /// 
    /// 작동 원리:
    /// 1. Attach: 엔티티를 Change Tracker에 등록
    /// 2. State를 Modified로 변경: 모든 속성이 변경된 것으로 표시
    /// 3. SaveChangesAsync 호출 시 UPDATE 쿼리 생성
    /// </summary>
    public virtual void Update(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // 엔티티가 Change Tracker에 없으면 Attach
        _dbSet.Attach(entity);
        // EntityState를 Modified로 변경
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// 여러 엔티티를 일괄 수정합니다.
    /// </summary>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        _dbSet.UpdateRange(entities);
    }

    // ============================================================
    // 삭제 (Delete) 메서드들
    // ============================================================

    /// <summary>
    /// 엔티티를 삭제합니다.
    /// 
    /// 작동 원리:
    /// EntityState를 Deleted로 변경하여 SaveChangesAsync 시 DELETE 쿼리 생성
    /// </summary>
    public virtual void Remove(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        _dbSet.Remove(entity);
    }

    /// <summary>
    /// 여러 엔티티를 일괄 삭제합니다.
    /// </summary>
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// ID로 엔티티를 삭제합니다.
    /// 
    /// 작동 방식:
    /// 1. ID로 엔티티 조회
    /// 2. 존재하면 Remove 호출
    /// 3. 삭제 여부 반환
    /// </summary>
    public virtual async Task<bool> RemoveByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        Remove(entity);
        return true;
    }
}
