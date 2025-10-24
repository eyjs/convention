using LocalRAG.Entities;

namespace LocalRAG.Repositories;

public interface IConventionRepository : IRepository<Convention>
{
    Task<IEnumerable<Convention>> GetConventionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Convention>> GetConventionsByTypeAsync(string conventionType, CancellationToken cancellationToken = default);
    Task<Convention?> GetConventionWithDetailsAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Convention>> GetActiveConventionsAsync(CancellationToken cancellationToken = default);
}
