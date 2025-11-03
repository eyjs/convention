using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// Convention Repository 구현체
/// 
/// 주요 기능:
/// 1. 기본 CRUD는 Repository<Convention>에서 상속
/// 2. Convention 엔티티에 특화된 복잡한 쿼리 구현
/// 3. Eager Loading을 통한 연관 데이터 로딩
/// </summary>
public class ConventionRepository : Repository<Convention>, IConventionRepository
{
    public ConventionRepository(ConventionDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 특정 기간 내의 행사를 조회합니다.
    /// 
    /// 작동 방식:
    /// - StartDate가 조회 범위와 겹치는 행사를 필터링
    /// - 날짜순으로 정렬하여 반환
    /// </summary>
    public async Task<IEnumerable<Convention>> GetConventionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.StartDate.HasValue &&
                       c.StartDate >= startDate &&
                       c.StartDate <= endDate)
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 행사 타입별로 조회합니다.
    /// 
    /// 예시: "DOMESTIC", "OVERSEAS"
    /// </summary>
    public async Task<IEnumerable<Convention>> GetConventionsByTypeAsync(
        string conventionType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.ConventionType == conventionType)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 행사와 관련된 모든 데이터를 포함하여 조회합니다.
    ///
    /// Eager Loading 설명:
    /// - Include: 관련 엔티티를 함께 로딩 (N+1 문제 방지)
    /// - ThenInclude: 2단계 이상 깊이의 관련 엔티티 로딩
    ///
    /// 장점: 한 번의 쿼리로 모든 관련 데이터 로딩 (성능 향상)
    /// 단점: 너무 많은 데이터를 로딩하면 메모리 사용량 증가
    /// </summary>
    public async Task<Convention?> GetConventionWithDetailsAsync(
        int conventionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.UserConventions)           // 참석자 매핑 포함
                .ThenInclude(uc => uc.User)            // 참석자 정보 포함
                    .ThenInclude(u => u.GuestAttributes) // 참석자의 속성 포함
            .Include(c => c.ScheduleTemplates)         // 전체 일정 템플릿 포함
                .ThenInclude(st => st.ScheduleItems)   // 템플릿의 개별 항목 포함
            .Include(c => c.Features)                  // 기능 포함
            .Include(c => c.Owners)                    // 담당자 포함
            .Include(c => c.Menus)                     // 메뉴 포함
                .ThenInclude(m => m.Sections)          // 메뉴의 섹션 포함
            .FirstOrDefaultAsync(c => c.Id == conventionId, cancellationToken);
    }

    /// <summary>
    /// 삭제되지 않은 활성 행사만 조회합니다.
    /// 
    /// 비즈니스 로직:
    /// - DeleteYn이 "N"인 행사만 필터링
    /// - 최신 행사부터 정렬
    /// </summary>
    public async Task<IEnumerable<Convention>> GetActiveConventionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.DeleteYn == "N")
            .OrderByDescending(c => c.RegDtm)
            .ToListAsync(cancellationToken);
    }
}
