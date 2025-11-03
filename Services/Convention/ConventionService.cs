using LocalRAG.Entities;
using LocalRAG.Repositories;
using ConventionModel = LocalRAG.Entities.Convention;

namespace LocalRAG.Services.Convention;

/// <summary>
/// Convention 서비스 예시
/// 
/// Repository 패턴 사용 예시를 보여주는 서비스입니다.
/// 실제 비즈니스 로직은 이런 서비스 레이어에서 구현합니다.
/// </summary>
public class ConventionService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 생성자 주입(Constructor Injection)
    /// 
    /// DI 컨테이너가 자동으로 IUnitOfWork 구현체를 주입합니다.
    /// </summary>
    public ConventionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    // ============================================================
    // 예시 1: 단순 조회
    // ============================================================

    /// <summary>
    /// 활성 행사 목록을 조회합니다.
    /// </summary>
    public async Task<IEnumerable<ConventionModel>> GetActiveConventionsAsync()
    {
        // Repository의 특화 메서드 사용
        return await _unitOfWork.Conventions.GetActiveConventionsAsync();
    }

    /// <summary>
    /// 행사 상세 정보를 조회합니다.
    /// </summary>
    public async Task<ConventionModel?> GetConventionDetailsAsync(int conventionId)
    {
        // Eager Loading으로 관련 데이터까지 한 번에 조회
        return await _unitOfWork.Conventions.GetConventionWithDetailsAsync(conventionId);
    }

    // ============================================================
    // 예시 2: 단일 엔티티 생성
    // ============================================================

    /// <summary>
    /// 새로운 행사를 생성합니다.
    /// 
    /// 트랜잭션 처리:
    /// 1. AddAsync로 Change Tracker에 등록
    /// 2. SaveChangesAsync로 데이터베이스에 커밋
    /// </summary>
    public async Task<ConventionModel> CreateConventionAsync(ConventionModel convention)
    {
        // 비즈니스 로직: 기본값 설정
        convention.RegDtm = DateTime.Now;
        convention.DeleteYn = "N";

        // Repository를 통한 데이터 추가
        await _unitOfWork.Conventions.AddAsync(convention);
        
        // Unit of Work로 변경사항 저장
        await _unitOfWork.SaveChangesAsync();

        return convention;
    }

    // ============================================================
    // 예시 3: 복잡한 비즈니스 로직 (여러 엔티티 처리)
    // ============================================================

    /// <summary>
    /// 행사와 담당자를 함께 생성합니다.
    /// 
    /// Unit of Work 패턴의 장점:
    /// - 여러 Repository의 작업을 하나의 트랜잭션으로 묶음
    /// - 모두 성공하거나 모두 실패 (원자성 보장)
    /// </summary>
    public async Task<ConventionModel> CreateConventionWithOwnersAsync(
        ConventionModel convention,
        List<Owner> owners)
    {
        // 1. 행사 생성
        convention.RegDtm = DateTime.Now;
        convention.DeleteYn = "N";
        await _unitOfWork.Conventions.AddAsync(convention);
        
        // 먼저 저장하여 Convention.Id 생성
        await _unitOfWork.SaveChangesAsync();

        // 2. 담당자 추가 (Convention.Id가 필요)
        foreach (var owner in owners)
        {
            owner.ConventionId = convention.Id;
            await _unitOfWork.Owners.AddAsync(owner);
        }

        // 3. 한 번에 커밋
        await _unitOfWork.SaveChangesAsync();

        return convention;
    }

    // ============================================================
    // 예시 4: 명시적 트랜잭션 제어
    // ============================================================

    /// <summary>
    /// 참석자를 등록하는 작업
    ///
    /// 명시적 트랜잭션 사용 시나리오:
    /// - 여러 SaveChangesAsync 호출이 필요한 경우
    /// - 중간 결과를 확인하고 조건부로 진행해야 하는 경우
    /// </summary>
    public async Task RegisterGuestAsync(Entities.User user, int conventionId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. User 추가
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // 2. UserConvention 추가
            var userConvention = new Entities.UserConvention
            {
                UserId = user.Id,
                ConventionId = conventionId,
                AccessToken = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.UserConventions.AddAsync(userConvention);
            await _unitOfWork.SaveChangesAsync();

            // 3. 참석자 속성 추가
            await _unitOfWork.GuestAttributes.UpsertAttributeAsync(
                user.Id,
                "registration_date",
                DateTime.Now.ToString("yyyy-MM-dd"));
            await _unitOfWork.SaveChangesAsync();

            // 모든 작업 성공 - 커밋
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            // 하나라도 실패하면 전체 롤백
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    // ============================================================
    // 예시 5: 수정 작업
    // ============================================================

    /// <summary>
    /// 행사 정보를 수정합니다.
    /// 
    /// 수정 패턴:
    /// 1. 조회: 수정할 엔티티 가져오기
    /// 2. 변경: 속성 값 수정
    /// 3. Update: Change Tracker에 수정 상태 등록
    /// 4. SaveChanges: 데이터베이스에 반영
    /// </summary>
    public async Task<bool> UpdateConventionAsync(int conventionId, ConventionModel updatedData)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
        {
            return false;
        }

        // 속성 업데이트
        convention.Title = updatedData.Title;
        convention.ConventionType = updatedData.ConventionType;
        convention.StartDate = updatedData.StartDate;
        convention.EndDate = updatedData.EndDate;
        convention.ConventionImg = updatedData.ConventionImg;

        // 수정 상태로 표시
        _unitOfWork.Conventions.Update(convention);

        // 저장
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ============================================================
    // 예시 6: 소프트 삭제 (Soft Delete)
    // ============================================================

    /// <summary>
    /// 행사를 소프트 삭제합니다.
    /// 
    /// 소프트 삭제란?
    /// - 실제 데이터를 삭제하지 않고 DeleteYn 플래그만 변경
    /// - 데이터 복구 가능
    /// - 이력 추적 용이
    /// </summary>
    public async Task<bool> SoftDeleteConventionAsync(int conventionId)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
        {
            return false;
        }

        // DeleteYn 플래그 변경
        convention.DeleteYn = "Y";
        _unitOfWork.Conventions.Update(convention);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ============================================================
    // 예시 7: 페이징 처리
    // ============================================================

    /// <summary>
    /// 행사 목록을 페이징하여 조회합니다.
    /// 
    /// 페이징 처리가 중요한 이유:
    /// - 대량 데이터 조회 시 성능 향상
    /// - 메모리 사용량 감소
    /// - 사용자 경험 개선 (빠른 응답)
    /// </summary>
    public async Task<(IEnumerable<ConventionModel> Items, int TotalCount, int TotalPages)> 
        GetConventionsPagedAsync(int pageNumber, int pageSize, string? conventionType = null)
    {
        // 조건 생성
        System.Linq.Expressions.Expression<Func<ConventionModel, bool>>? predicate = null;
        
        if (!string.IsNullOrEmpty(conventionType))
        {
            predicate = c => c.ConventionType == conventionType && c.DeleteYn == "N";
        }
        else
        {
            predicate = c => c.DeleteYn == "N";
        }

        // 페이징 조회
        var (items, totalCount) = await _unitOfWork.Conventions.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate);

        // 전체 페이지 수 계산
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return (items, totalCount, totalPages);
    }

    // ============================================================
    // 예시 8: 검색 기능
    // ============================================================

    /// <summary>
    /// 참석자를 검색합니다.
    ///
    /// Repository 특화 메서드 활용
    /// </summary>
    public async Task<IEnumerable<Entities.User>> SearchGuestsAsync(
        string searchKeyword,
        int? conventionId = null)
    {
        return await _unitOfWork.Users.SearchUsersByNameAsync(
            searchKeyword);
    }

    // ============================================================
    // 예시 9: 일괄 처리
    // ============================================================

    /// <summary>
    /// 여러 참석자를 일괄 등록합니다.
    ///
    /// 일괄 처리의 장점:
    /// - 한 번의 트랜잭션으로 처리
    /// - 데이터베이스 왕복 횟수 감소
    /// - 성능 향상
    /// </summary>
    public async Task<int> BulkRegisterGuestsAsync(List<Entities.User> users, int conventionId)
    {
        // User 일괄 추가
        await _unitOfWork.Users.AddRangeAsync(users);
        await _unitOfWork.SaveChangesAsync();

        // UserConvention 생성
        var userConventions = new List<Entities.UserConvention>();
        foreach (var user in users)
        {
            userConventions.Add(new Entities.UserConvention
            {
                UserId = user.Id,
                ConventionId = conventionId,
                AccessToken = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            });
        }

        // UserConvention 일괄 추가
        await _unitOfWork.UserConventions.AddRangeAsync(userConventions);

        // 한 번에 저장
        return await _unitOfWork.SaveChangesAsync();
    }

    // ============================================================
    // 예시 10: 복합 조회 (여러 Repository 활용)
    // ============================================================

    /// <summary>
    /// 행사의 통계 정보를 조회합니다.
    /// 
    /// 여러 Repository를 조합하여 복합 데이터 생성
    /// </summary>
    public async Task<ConventionStatistics> GetConventionStatisticsAsync(int conventionId)
    {
        // 1. 행사 정보
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
        {
            throw new KeyNotFoundException($"Convention {conventionId} not found.");
        }

        // 2. 참석자 수
        var guestCount = await _unitOfWork.UserConventions.CountAsync(
            uc => uc.ConventionId == conventionId);

        // 3. 일정 수
        var scheduleCount = await _unitOfWork.Schedules.CountAsync(
            s => s.ConventionId == conventionId);

        // 4. 활성화된 기능 수
        var enabledFeatureCount = await _unitOfWork.Features.CountAsync(
            f => f.ConventionId == conventionId && f.IsActive);

        return new ConventionStatistics
        {
            ConventionId = conventionId,
            ConventionTitle = convention.Title,
            TotalGuests = guestCount,
            TotalSchedules = scheduleCount,
            EnabledFeatures = enabledFeatureCount
        };
    }
}

/// <summary>
/// 행사 통계 DTO
/// </summary>
public class ConventionStatistics
{
    public int ConventionId { get; set; }
    public string ConventionTitle { get; set; } = string.Empty;
    public int TotalGuests { get; set; }
    public int TotalSchedules { get; set; }
    public int EnabledFeatures { get; set; }
}
