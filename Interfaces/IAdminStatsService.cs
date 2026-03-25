namespace LocalRAG.Interfaces;

public interface IAdminStatsService
{
    Task<object> GetConventionStatsAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<object> GetScheduleAssignedUsersAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<object> GetScheduleCourseUsersAsync(int conventionId, int scheduleTemplateId, CancellationToken cancellationToken = default);
    Task<object> GetAttributeUsersAsync(int conventionId, string attributeKey, string attributeValue, CancellationToken cancellationToken = default);
}
