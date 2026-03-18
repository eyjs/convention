using LocalRAG.Interfaces;
using LocalRAG.Repositories;

using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Shared;

public class ChecklistService : IChecklistService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChecklistService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 체크리스트 상태 구축
    /// </summary>
    public async Task<object?> BuildChecklistStatusAsync(int userId, int conventionId)
    {
        var actions = await _unitOfWork.ConventionActions.Query
            .Where(a => a.ConventionId == conventionId && a.IsActive)
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        if (actions.Count == 0)
            return null;

        var statuses = await _unitOfWork.UserActionStatuses.Query
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var statusDict = statuses.ToDictionary(s => s.ConventionActionId, s => s);

        var items = new List<object>();
        int completedCount = 0;

        foreach (var action in actions)
        {
            var status = statusDict.GetValueOrDefault(action.Id);
            bool isComplete = status?.IsComplete ?? false;

            if (isComplete)
                completedCount++;

            items.Add(new
            {
                actionId = action.Id,
                title = action.Title,
                isComplete,
                deadline = action.Deadline,
                navigateTo = action.MapsTo,
                orderNum = action.OrderNum
            });
        }

        DateTime? overallDeadline = actions
            .Where(a => {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false) && a.Deadline.HasValue;
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? completedCount * 100 / totalItems : 0;

        return new
        {
            totalItems,
            completedItems = completedCount,
            progressPercentage,
            overallDeadline,
            items = items
        };
    }
}
