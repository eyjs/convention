using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using System.Text.Json;

namespace LocalRAG.Services.Convention;

/// <summary>
/// 사용자 액션 서비스 구현체
/// UserActionController의 비즈니스 로직 담당
/// </summary>
public class UserActionService : IUserActionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConventionDbContext _context;
    private readonly IActionOrchestrationService _orchestrationService;
    private readonly ILogger<UserActionService> _logger;

    public UserActionService(
        IUnitOfWork unitOfWork,
        ConventionDbContext context,
        IActionOrchestrationService orchestrationService,
        ILogger<UserActionService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    public async Task<object> GetUrgentActionsAsync(int conventionId)
    {
        var now = DateTime.UtcNow;

        var actions = await _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.Deadline.HasValue &&
                       a.Deadline.Value > now)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
                a.BehaviorType,
                a.TargetId,
                IconClass = a.IconClass ?? (a.Template == null ? null : a.Template.IconClass),
                Category = a.Category ?? (a.Template == null ? null : a.Template.Category)
            })
            .ToListAsync();

        return actions;
    }

    public async Task<object> GetAllActionsAsync(int conventionId, string? targetLocation, string? actionCategory, bool? isActive = null)
    {
        var query = _context.ConventionActions
            .Include(a => a.Template)
            .Where(a => a.ConventionId == conventionId);

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }
        else
        {
            query = query.Where(a => a.IsActive);
        }

        if (!string.IsNullOrEmpty(targetLocation))
        {
            var locations = targetLocation.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(l => l.Trim())
                                         .ToList();
            if (locations.Any())
            {
                query = query.Where(a => a.TargetLocation != null && locations.Contains(a.TargetLocation));
            }
        }

        if (!string.IsNullOrEmpty(actionCategory))
        {
            query = query.Where(a => a.ActionCategory == actionCategory);
        }

        var actions = await query
            .OrderBy(a => a.Category)
            .ThenBy(a => a.OrderNum)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Deadline,
                a.MapsTo,
                a.IsRequired,
                a.ActionCategory,
                a.TargetLocation,
                a.ConfigJson,
                a.BehaviorType,
                a.TargetId,
                IconClass = a.IconClass ?? (a.Template == null ? null : a.Template.IconClass),
                Category = a.Category ?? (a.Template == null ? null : a.Template.Category)
            })
            .ToListAsync();

        return actions;
    }

    public async Task<object> GetUserActionStatusesAsync(int conventionId, int userId)
    {
        var statuses = await _context.UserActionStatuses
            .Where(s => s.UserId == userId && s.ConventionAction != null && s.ConventionAction.ConventionId == conventionId)
            .ToListAsync();

        return statuses;
    }

    public async Task<object> GetUserChecklistAsync(int conventionId, int userId)
    {
        var checklist = await _orchestrationService.GetUserActionsAsync(conventionId, userId);
        return checklist;
    }

    public async Task<object?> GetActionDetailAsync(int conventionId, int actionId)
    {
        var action = await _context.ConventionActions
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.ConventionId == conventionId &&
                                    a.Id == actionId &&
                                    a.IsActive);

        if (action == null)
            return null;

        return new
        {
            action.Id,
            action.Title,
            action.Deadline,
            action.MapsTo,
            action.ConfigJson,
            action.IsRequired,
            action.ActionCategory,
            action.TargetLocation,
            action.BehaviorType,
            action.TargetId,
            IconClass = action.IconClass ?? action.Template?.IconClass,
            Category = action.Category ?? action.Template?.Category
        };
    }

    public async Task<object?> CompleteActionAsync(int conventionId, int actionId, int userId, string? responseDataJson)
    {
        var action = await _unitOfWork.ConventionActions.GetAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
            return null;

        var status = await _context.UserActionStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = action.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.UserActionStatuses.AddAsync(status);
        }

        status.IsComplete = true;
        status.CompletedAt = DateTime.UtcNow;
        status.ResponseDataJson = responseDataJson;
        status.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new { message = "액션이 완료되었습니다." };
    }

    public async Task<object?> ToggleActionAsync(int conventionId, int actionId, int userId, bool isComplete)
    {
        var action = await _unitOfWork.ConventionActions.GetAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
            return null;

        var status = await _context.UserActionStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = action.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.UserActionStatuses.AddAsync(status);
        }

        status.IsComplete = isComplete;
        status.CompletedAt = isComplete ? DateTime.UtcNow : null;
        status.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new
        {
            message = isComplete ? "액션이 완료되었습니다." : "완료가 취소되었습니다.",
            isComplete = status.IsComplete
        };
    }

    public async Task<(object? Result, string? Error, bool NotFound)> SubmitActionAsync(
        int conventionId, int actionId, int userId, JsonElement submissionData)
    {
        var action = await _unitOfWork.ConventionActions.GetAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
            return (null, "액션을 찾을 수 없습니다.", true);

        if (action.BehaviorType != BehaviorType.FormBuilder)
            return (null, "이 액션은 FormBuilder 타입이 아닙니다.", false);

        var submission = await _context.ActionSubmissions
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (submission != null)
        {
            submission.SubmissionDataJson = submissionData.ToString();
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            submission = new ActionSubmission
            {
                ConventionActionId = actionId,
                UserId = userId,
                SubmissionDataJson = submissionData.ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            await _unitOfWork.ActionSubmissions.AddAsync(submission);
        }

        var status = await _context.UserActionStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == actionId);

        if (status == null)
        {
            status = new UserActionStatus
            {
                UserId = userId,
                ConventionActionId = actionId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.UserActionStatuses.AddAsync(status);
        }

        status.IsComplete = true;
        status.CompletedAt = DateTime.UtcNow;
        status.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return (new { message = "제출이 완료되었습니다." }, null, false);
    }

    public async Task<JsonElement?> GetUserSubmissionAsync(int actionId, int userId)
    {
        var submission = await _context.ActionSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ConventionActionId == actionId && s.UserId == userId);

        if (submission == null)
            return null;

        var jsonData = JsonDocument.Parse(submission.SubmissionDataJson);
        return jsonData.RootElement;
    }

    public async Task<(object? Result, string? Error, bool NotFound)> GetAllSubmissionsAsync(int conventionId, int actionId)
    {
        var action = await _unitOfWork.ConventionActions.GetAsync(a => a.Id == actionId && a.ConventionId == conventionId);
        if (action == null)
            return (null, "액션을 찾을 수 없습니다.", true);

        if (action.BehaviorType != BehaviorType.FormBuilder)
            return (null, "이 액션은 FormBuilder 타입이 아닙니다.", false);

        var submissionsRaw = await _context.ActionSubmissions
            .Include(s => s.User)
            .Where(s => s.ConventionActionId == actionId)
            .ToListAsync();

        var submissions = submissionsRaw.Select(s => new
        {
            s.Id,
            s.UserId,
            UserName = s.User.Name,
            UserEmail = s.User.Email,
            SubmissionData = JsonDocument.Parse(s.SubmissionDataJson).RootElement,
            s.SubmittedAt,
            s.UpdatedAt
        }).ToList();

        return (submissions, null, false);
    }

    public async Task<object> GetChecklistStatusAsync(int conventionId, int userId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.Deadline.HasValue)
            .OrderBy(a => a.Deadline)
            .ThenBy(a => a.OrderNum)
            .ToListAsync();

        if (actions.Count == 0)
            return new { totalItems = 0, completedItems = 0, progressPercentage = 0, items = new List<object>() };

        var statuses = await _context.UserActionStatuses
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
                isComplete = isComplete,
                deadline = action.Deadline,
                navigateTo = action.MapsTo,
                orderNum = action.OrderNum,
                behaviorType = action.BehaviorType,
                targetId = action.TargetId
            });
        }

        DateTime? overallDeadline = actions
            .Where(a =>
            {
                var status = statusDict.GetValueOrDefault(a.Id);
                return !(status?.IsComplete ?? false);
            })
            .OrderBy(a => a.Deadline)
            .FirstOrDefault()?.Deadline;

        int totalItems = actions.Count;
        int progressPercentage = totalItems > 0 ? (completedCount * 100 / totalItems) : 0;

        return new
        {
            totalItems,
            completedItems = completedCount,
            progressPercentage,
            overallDeadline,
            items
        };
    }

    public async Task<object> GetMenuActionsAsync(int conventionId)
    {
        var actions = await _context.ConventionActions
            .Where(a => a.ConventionId == conventionId &&
                       a.IsActive &&
                       a.ActionCategory == "MENU")
            .OrderBy(a => a.OrderNum)
            .ThenBy(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.MapsTo,
                a.OrderNum,
                a.BehaviorType,
                a.TargetId,
                a.ConfigJson
            })
            .ToListAsync();

        return actions;
    }
}
