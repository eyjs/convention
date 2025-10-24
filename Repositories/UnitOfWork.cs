using LocalRAG.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace LocalRAG.Repositories;

/// <summary>
/// Unit of Work 구현체
/// 
/// Unit of Work 패턴의 핵심 개념:
/// 1. 여러 Repository의 변경사항을 하나의 트랜잭션으로 묶음
/// 2. 모든 작업이 성공하면 커밋, 하나라도 실패하면 롤백
/// 3. DbContext의 수명 관리 및 트랜잭션 제어
/// 
/// 사용 예시:
/// using (var uow = new UnitOfWork(context))
/// {
///     await uow.Conventions.AddAsync(convention);
///     await uow.Guests.AddAsync(guest);
///     await uow.SaveChangesAsync(); // 모두 성공 또는 모두 실패
/// }
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ConventionDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repository 인스턴스들 (Lazy Initialization)
    private IConventionRepository? _conventions;
    private IGuestRepository? _guests;
    private IScheduleRepository? _schedules;
    private IGuestAttributeRepository? _guestAttributes;
    private IFeatureRepository? _features;
    private IMenuRepository? _menus;
    private ISectionRepository? _sections;
    private IOwnerRepository? _owners;
    private IVectorStoreRepository? _vectorStores;
    private IRepository<Entities.Action.ConventionAction>? _conventionActions;
    private IRepository<Entities.GuestActionStatus>? _guestActionStatuses;

    public UnitOfWork(ConventionDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ============================================================
    // Repository Properties (Lazy Initialization 패턴)
    // ============================================================
    // 
    // Lazy Initialization이란?
    // - 실제 사용될 때까지 객체 생성을 지연
    // - 메모리 효율성: 사용하지 않는 Repository는 생성하지 않음
    // - 성능 향상: 초기화 비용을 사용 시점으로 분산
    //
    // 작동 방식:
    // 1. _conventions가 null이면 새로 생성
    // 2. 이미 생성되어 있으면 기존 인스턴스 반환
    // 3. ??= 연산자: null일 때만 할당 (null-coalescing assignment)

    public IConventionRepository Conventions =>
        _conventions ??= new ConventionRepository(_context);

    public IGuestRepository Guests =>
        _guests ??= new GuestRepository(_context);

    public IScheduleRepository Schedules =>
        _schedules ??= new ScheduleRepository(_context);

    public IGuestAttributeRepository GuestAttributes =>
        _guestAttributes ??= new GuestAttributeRepository(_context);

    public IFeatureRepository Features =>
        _features ??= new FeatureRepository(_context);

    public IMenuRepository Menus =>
        _menus ??= new MenuRepository(_context);

    public ISectionRepository Sections =>
        _sections ??= new SectionRepository(_context);

    public IOwnerRepository Owners =>
        _owners ??= new OwnerRepository(_context);

    public IVectorStoreRepository VectorStores =>
        _vectorStores ??= new VectorStoreRepository(_context);

    public IRepository<Entities.Action.ConventionAction> ConventionActions =>
        _conventionActions ??= new Repository<Entities.Action.ConventionAction>(_context);

    public IRepository<Entities.GuestActionStatus> GuestActionStatuses =>
        _guestActionStatuses ??= new Repository<Entities.GuestActionStatus>(_context);

    // ============================================================
    // Transaction Methods
    // ============================================================

    /// <summary>
    /// 모든 변경사항을 데이터베이스에 저장합니다.
    /// 
    /// 작동 원리:
    /// 1. EF Core의 Change Tracker가 추적한 모든 변경사항 확인
    /// 2. 변경사항들을 SQL 명령으로 변환
    /// 3. 데이터베이스에 한 번에 전송 (단일 트랜잭션)
    /// 4. 성공: 변경된 행의 수 반환
    /// 5. 실패: 예외 발생 및 자동 롤백
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // 예외 발생 시 Change Tracker의 변경사항은 유지됨
            // 필요시 재시도하거나 수동으로 롤백 가능
            throw;
        }
    }

    /// <summary>
    /// 명시적인 트랜잭션을 시작합니다.
    /// 
    /// 사용 시나리오:
    /// - 복잡한 비즈니스 로직에서 여러 SaveChangesAsync 호출이 필요한 경우
    /// - 일부 작업 후 조건에 따라 커밋/롤백 결정이 필요한 경우
    /// 
    /// 예시:
    /// await BeginTransactionAsync();
    /// try
    /// {
    ///     await Conventions.AddAsync(convention);
    ///     await SaveChangesAsync();
    ///     
    ///     // 추가 로직...
    ///     
    ///     await CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     await RollbackTransactionAsync();
    ///     throw;
    /// }
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// 트랜잭션을 커밋합니다.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to commit.");
        }

        try
        {
            // 먼저 변경사항 저장
            await _context.SaveChangesAsync(cancellationToken);
            // 그 다음 트랜잭션 커밋
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // 커밋 실패 시 자동 롤백
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// 트랜잭션을 롤백합니다.
    /// 
    /// 롤백 효과:
    /// - 데이터베이스: 트랜잭션 시작 이후의 모든 변경사항 취소
    /// - Change Tracker: 추적 중인 엔티티 상태는 유지됨 (주의!)
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // ============================================================
    // Dispose Pattern (리소스 정리)
    // ============================================================
    
    /// <summary>
    /// 리소스를 정리합니다.
    /// 
    /// 정리 대상:
    /// - DbContext: 데이터베이스 연결 해제
    /// - Transaction: 진행 중인 트랜잭션 롤백
    /// 
    /// using 문과 함께 사용하면 자동으로 호출됩니다.
    /// </summary>
    public void Dispose()
    {
        // 트랜잭션이 진행 중이면 롤백
        _transaction?.Dispose();
        
        // DbContext 정리
        _context?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
