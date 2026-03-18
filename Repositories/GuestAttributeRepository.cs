using LocalRAG.Data;
using LocalRAG.Entities;
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

    public async Task<IEnumerable<GuestAttribute>> GetAttributesByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ga => ga.UserId == userId)
            .OrderBy(ga => ga.AttributeKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<GuestAttribute?> GetAttributeByKeyAsync(
        int userId,
        string attributeKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ga => ga.UserId == userId && ga.AttributeKey == attributeKey,
                cancellationToken);
    }

    public async Task UpsertAttributeAsync(
        int userId,
        string attributeKey,
        string attributeValue,
        CancellationToken cancellationToken = default)
    {
        var existingAttribute = await _dbSet
            .FirstOrDefaultAsync(
                ga => ga.UserId == userId && ga.AttributeKey == attributeKey,
                cancellationToken);

        if (existingAttribute != null)
        {
            existingAttribute.AttributeValue = attributeValue;
        }
        else
        {
            _dbSet.Add(new GuestAttribute
            {
                UserId = userId,
                AttributeKey = attributeKey,
                AttributeValue = attributeValue
            });
        }
    }
}
