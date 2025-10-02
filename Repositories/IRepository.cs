using System.Linq.Expressions;

namespace LocalRAG.Repositories;

/// <summary>
/// 제네릭 Repository 인터페이스
/// 모든 엔티티에 공통으로 사용되는 기본 CRUD 메서드를 정의합니다.
/// </summary>
/// <typeparam name="TEntity">엔티티 타입</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    // --- 조회 (Read) ---

    /// <summary>
    /// ID로 단일 엔티티를 조회합니다.
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 단일 엔티티를 조회합니다.
    /// </summary>
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 모든 엔티티를 조회합니다.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 모든 엔티티를 조회합니다.
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 페이징 처리된 결과를 조회합니다.
    /// </summary>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 엔티티가 존재하는지 확인합니다.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 조건에 맞는 엔티티의 개수를 반환합니다.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    // --- 생성 (Create) ---

    /// <summary>
    /// 새로운 엔티티를 추가합니다.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 엔티티를 일괄 추가합니다.
    /// </summary>
    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    // --- 수정 (Update) ---

    /// <summary>
    /// 엔티티를 수정합니다.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// 여러 엔티티를 일괄 수정합니다.
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);

    // --- 삭제 (Delete) ---

    /// <summary>
    /// 엔티티를 삭제합니다.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// 여러 엔티티를 일괄 삭제합니다.
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// ID로 엔티티를 삭제합니다.
    /// </summary>
    Task<bool> RemoveByIdAsync(int id, CancellationToken cancellationToken = default);
}
