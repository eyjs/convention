using LocalRAG.Data;
using LocalRAG.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// GuestAttribute Repository 구현체
/// </summary>
public class GuestAttributeRepository : Repository<GuestAttribute>, IGuestAttributeRepository
{
    public GuestAttributeRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GuestAttribute>> GetAttributesByGuestIdAsync(
        int guestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ga => ga.GuestId == guestId)
            .OrderBy(ga => ga.AttributeKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<GuestAttribute?> GetAttributeByKeyAsync(
        int guestId,
        string attributeKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ga => ga.GuestId == guestId && ga.AttributeKey == attributeKey,
                cancellationToken);
    }

    /// <summary>
    /// 참석자의 속성을 Upsert합니다.
    /// 
    /// Upsert 패턴 설명:
    /// 1. 기존 데이터 존재 여부 확인
    /// 2. 있으면 Update, 없으면 Insert
    /// 
    /// 주의사항:
    /// - SaveChangesAsync는 UnitOfWork에서 호출해야 함
    /// </summary>
    public async Task UpsertAttributeAsync(
        int guestId,
        string attributeKey,
        string attributeValue,
        CancellationToken cancellationToken = default)
    {
        var existingAttribute = await _dbSet
            .FirstOrDefaultAsync(
                ga => ga.GuestId == guestId && ga.AttributeKey == attributeKey,
                cancellationToken);

        if (existingAttribute != null)
        {
            // Update: 기존 값 수정
            existingAttribute.AttributeValue = attributeValue;
            _context.Entry(existingAttribute).State = EntityState.Modified;
        }
        else
        {
            // Insert: 새로운 속성 추가
            var newAttribute = new GuestAttribute
            {
                GuestId = guestId,
                AttributeKey = attributeKey,
                AttributeValue = attributeValue
            };
            await _dbSet.AddAsync(newAttribute, cancellationToken);
        }
    }
}
