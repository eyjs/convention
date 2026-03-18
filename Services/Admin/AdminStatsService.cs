using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Admin;

public class AdminStatsService : IAdminStatsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminStatsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetConventionStatsAsync(int conventionId, CancellationToken cancellationToken = default)
    {
        var totalGuests = await _unitOfWork.UserConventions
            .CountAsync(uc => uc.ConventionId == conventionId, cancellationToken);

        var totalSchedules = await _unitOfWork.ScheduleTemplates
            .CountAsync(st => st.ConventionId == conventionId, cancellationToken);

        var scheduleAssignments = await _unitOfWork.GuestScheduleTemplates.Query
            .Include(gst => gst.User)
                .ThenInclude(u => u.UserConventions)
            .CountAsync(gst => gst.User!.UserConventions.Any(uc => uc.ConventionId == conventionId), cancellationToken);

        var recentGuests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .OrderByDescending(uc => uc.UserId)
            .Take(5)
            .Select(uc => new
            {
                Id = uc.UserId,
                Name = uc.User.Name,
                CorpPart = uc.User.CorpPart,
                Phone = uc.User.Phone
            })
            .ToListAsync(cancellationToken);

        var scheduleStats = await _unitOfWork.ScheduleTemplates.Query
            .Where(st => st.ConventionId == conventionId)
            .Select(st => new
            {
                st.Id,
                st.CourseName,
                ItemCount = st.ScheduleItems.Count,
                GuestCount = st.GuestScheduleTemplates.Count
            })
            .ToListAsync(cancellationToken);

        var attributeStats = await _unitOfWork.Users.Query
            .Where(u => u.UserConventions.Any(uc => uc.ConventionId == conventionId))
            .SelectMany(u => u.GuestAttributes)
            .GroupBy(ga => ga.AttributeKey)
            .Select(group => new
            {
                AttributeKey = group.Key,
                Values = group.GroupBy(ga => ga.AttributeValue)
                    .Select(vg => new { Value = vg.Key, Count = vg.Count() })
                    .OrderByDescending(v => v.Count)
                    .ToList(),
                TotalCount = group.Count()
            })
            .OrderByDescending(a => a.TotalCount)
            .ToListAsync(cancellationToken);

        var smsHistory = await _unitOfWork.SmsLogs.Query
            .Where(l => l.ConventionId == conventionId)
            .OrderByDescending(l => l.SentAt)
            .Take(20)
            .Select(l => new
            {
                l.Id,
                l.ReceiverName,
                l.Message,
                SentAt = l.SentAt.AddHours(9).ToString("HH:mm"),
                Status = "success",
                StatusText = "발송성공"
            })
            .ToListAsync(cancellationToken);

        return new
        {
            totalGuests,
            totalSchedules,
            scheduleAssignments,
            recentGuests,
            scheduleStats,
            attributeStats,
            smsHistory
        };
    }
}
