using LocalRAG.Models;

namespace LocalRAG.Repositories;

public interface IGuestRepository : IRepository<Guest>
{
    Task<IEnumerable<Guest>> GetGuestsByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<Guest?> GetGuestWithDetailsAsync(int guestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guest>> SearchGuestsByNameAsync(string guestName, int? conventionId = null, CancellationToken cancellationToken = default);
    Task<Guest?> GetGuestByResidentNumberAsync(string residentNumber, int conventionId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, List<ScheduleItem>>> GetSchedulesForGuestsAsync(IEnumerable<int> guestIds, CancellationToken cancellationToken = default);
}
