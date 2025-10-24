using LocalRAG.Entities;
using LocalRAG.Repositories;

namespace LocalRAG.Repositories;

/// <summary>
/// Unit of Work 인터페이스
/// 
/// Unit of Work 패턴이란?
/// - 여러 Repository의 작업을 하나의 트랜잭션으로 묶어서 관리하는 패턴
/// - 비즈니스 로직 단위로 트랜잭션을 관리할 수 있어 데이터 일관성 보장
/// 
/// 사용 예시:
/// await _unitOfWork.Conventions.AddAsync(convention);
/// await _unitOfWork.Guests.AddAsync(guest);
/// await _unitOfWork.SaveChangesAsync(); // 한 번의 호출로 모든 변경사항을 커밋
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // --- Repository Properties ---
    // 각 엔티티별 Repository에 접근할 수 있는 속성
    IConventionRepository Conventions { get; }
    IGuestRepository Guests { get; }
    IScheduleRepository Schedules { get; }
    IGuestAttributeRepository GuestAttributes { get; }
    IFeatureRepository Features { get; }
    IMenuRepository Menus { get; }
    ISectionRepository Sections { get; }
    IOwnerRepository Owners { get; }
    IVectorStoreRepository VectorStores { get; }
    IRepository<Entities.Action.ConventionAction> ConventionActions { get; }
    IRepository<Entities.GuestActionStatus> GuestActionStatuses { get; }


    // --- Transaction Methods ---

    /// <summary>
    /// 모든 변경사항을 데이터베이스에 저장합니다.
    /// 
    /// 작동 원리:
    /// 1. Change Tracker에 등록된 모든 변경사항 확인
    /// 2. 하나의 트랜잭션으로 모든 변경사항 커밋
    /// 3. 성공 시 변경된 행의 수 반환, 실패 시 롤백
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션을 시작합니다.
    /// 복잡한 비즈니스 로직에서 명시적인 트랜잭션 제어가 필요할 때 사용
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션을 커밋합니다.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션을 롤백합니다.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// ============================================================
// 각 엔티티별 Repository 인터페이스
// ============================================================


/// <summary>
/// Schedule Repository 인터페이스
/// </summary>
public interface IScheduleRepository : IRepository<Schedule>
{
    /// <summary>
    /// 특정 행사의 모든 일정을 순서대로 조회합니다.
    /// </summary>
    Task<IEnumerable<Schedule>> GetSchedulesByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 날짜의 일정을 조회합니다.
    /// </summary>
    Task<IEnumerable<Schedule>> GetSchedulesByDateAsync(
        DateTime date,
        int? conventionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 일정 그룹별로 조회합니다.
    /// </summary>
    Task<IEnumerable<Schedule>> GetSchedulesByGroupAsync(
        string group,
        int conventionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// GuestAttribute Repository 인터페이스
/// </summary>
public interface IGuestAttributeRepository : IRepository<GuestAttribute>
{
    /// <summary>
    /// 특정 참석자의 모든 속성을 조회합니다.
    /// </summary>
    Task<IEnumerable<GuestAttribute>> GetAttributesByGuestIdAsync(
        int guestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 키의 속성 값을 조회합니다.
    /// </summary>
    Task<GuestAttribute?> GetAttributeByKeyAsync(
        int guestId,
        string attributeKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 참석자의 속성을 Upsert(있으면 수정, 없으면 추가)합니다.
    /// </summary>
    Task UpsertAttributeAsync(
        int guestId,
        string attributeKey,
        string attributeValue,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Feature Repository 인터페이스
/// </summary>
public interface IFeatureRepository : IRepository<Feature>
{
    /// <summary>
    /// 특정 행사의 모든 기능을 조회합니다.
    /// </summary>
    Task<IEnumerable<Feature>> GetFeaturesByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 활성화된 기능만 조회합니다.
    /// </summary>
    Task<IEnumerable<Feature>> GetEnabledFeaturesAsync(
        int conventionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 기능이 활성화되어 있는지 확인합니다.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(
        int conventionId,
        string featureName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Menu Repository 인터페이스
/// </summary>
public interface IMenuRepository : IRepository<Menu>
{
    /// <summary>
    /// 특정 행사의 모든 메뉴를 순서대로 조회합니다.
    /// </summary>
    Task<IEnumerable<Menu>> GetMenusByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 메뉴와 섹션을 함께 조회합니다.
    /// </summary>
    Task<Menu?> GetMenuWithSectionsAsync(
        int menuId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Section Repository 인터페이스
/// </summary>
public interface ISectionRepository : IRepository<Section>
{
    /// <summary>
    /// 특정 메뉴의 모든 섹션을 순서대로 조회합니다.
    /// </summary>
    Task<IEnumerable<Section>> GetSectionsByMenuIdAsync(
        int menuId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Owner Repository 인터페이스
/// </summary>
public interface IOwnerRepository : IRepository<Owner>
{
    /// <summary>
    /// 특정 행사의 모든 담당자를 조회합니다.
    /// </summary>
    Task<IEnumerable<Owner>> GetOwnersByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// VectorStore Repository 인터페이스
/// </summary>
public interface IVectorStoreRepository : IRepository<VectorStore>
{
    /// <summary>
    /// 특정 행사의 모든 벡터 데이터를 조회합니다.
    /// </summary>
    Task<IEnumerable<VectorStore>> GetVectorsByConventionIdAsync(
        int conventionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 특정 소스 테이블의 벡터 데이터를 조회합니다.
    /// </summary>
    Task<IEnumerable<VectorStore>> GetVectorsBySourceAsync(
        string sourceTable,
        string sourceId,
        CancellationToken cancellationToken = default);
}