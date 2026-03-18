namespace LocalRAG.Interfaces;

public interface IAdminStatsService
{
    Task<object> GetConventionStatsAsync(int conventionId, CancellationToken cancellationToken = default);
}
