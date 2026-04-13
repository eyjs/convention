using LocalRAG.Entities;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

public class NotificationSendService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationSendService> _logger;

    public NotificationSendService(IUnitOfWork unitOfWork, ILogger<NotificationSendService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<object> SendAsync(int conventionId, int sentByUserId, SendNotificationRequest req)
    {
        // 대상 사용자 조회
        var query = _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId);

        if (req.TargetScope == "GROUP" && !string.IsNullOrEmpty(req.TargetGroupName))
            query = query.Where(uc => uc.GroupName == req.TargetGroupName);
        else if (req.TargetScope == "INDIVIDUAL" && req.TargetUserIds?.Any() == true)
            query = query.Where(uc => req.TargetUserIds.Contains(uc.UserId));

        var userIds = await query.Select(uc => uc.UserId).Distinct().ToListAsync();

        if (userIds.Count == 0)
            return new { success = false, message = "대상 사용자가 없습니다." };

        // 딥링크 자동 생성
        var linkUrl = req.LinkUrl;
        if (string.IsNullOrEmpty(linkUrl))
        {
            linkUrl = req.Type switch
            {
                "NOTICE" when req.ReferenceId.HasValue => $"/conventions/{conventionId}/board/{req.ReferenceId}",
                "SURVEY" when req.ReferenceId.HasValue => $"/conventions/{conventionId}/surveys/{req.ReferenceId}",
                "SCHEDULE" => $"/conventions/{conventionId}/schedule",
                "SEAT" => $"/conventions/{conventionId}/my-seat",
                _ => null,
            };
        }

        var notification = new Notification
        {
            ConventionId = conventionId,
            Type = req.Type,
            Title = req.Title,
            Body = req.Body,
            LinkUrl = linkUrl,
            ReferenceId = req.ReferenceId,
            TargetScope = req.TargetScope,
            TargetGroupName = req.TargetGroupName,
            SentByUserId = sentByUserId,
            CreatedAt = DateTime.UtcNow,
            UserNotifications = userIds.Select(uid => new UserNotification
            {
                UserId = uid,
                IsRead = false,
            }).ToList(),
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Notification sent: {Title} to {Count} users", req.Title, userIds.Count);

        return new { success = true, notificationId = notification.Id, recipientCount = userIds.Count };
    }

    public async Task<object> GetHistoryAsync(int conventionId)
    {
        var list = await _unitOfWork.Notifications.Query
            .Where(n => n.ConventionId == conventionId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new
            {
                n.Id, n.Type, n.Title, n.Body, n.LinkUrl, n.TargetScope, n.CreatedAt,
                TotalCount = n.UserNotifications.Count,
                ReadCount = n.UserNotifications.Count(un => un.IsRead),
            })
            .ToListAsync();

        return list;
    }

    public async Task<object?> GetStatsAsync(int notificationId)
    {
        var n = await _unitOfWork.Notifications.Query
            .Where(x => x.Id == notificationId)
            .Select(x => new
            {
                x.Id, x.Title, x.Type, x.CreatedAt,
                Total = x.UserNotifications.Count,
                Read = x.UserNotifications.Count(un => un.IsRead),
                Unread = x.UserNotifications.Count(un => !un.IsRead),
                ReadUsers = x.UserNotifications.Where(un => un.IsRead).Select(un => new { un.User.Name, un.ReadAt }).ToList(),
                UnreadUsers = x.UserNotifications.Where(un => !un.IsRead).Select(un => new { un.User.Name }).ToList(),
            })
            .FirstOrDefaultAsync();

        return n;
    }

    // 사용자용
    public async Task<object> GetMyNotificationsAsync(int userId, int conventionId)
    {
        var list = await _unitOfWork.UserNotifications.Query
            .Where(un => un.UserId == userId && un.Notification.ConventionId == conventionId)
            .OrderByDescending(un => un.Notification.CreatedAt)
            .Select(un => new
            {
                un.Id, un.NotificationId, un.IsRead, un.ReadAt,
                un.Notification.Type, un.Notification.Title, un.Notification.Body,
                un.Notification.LinkUrl, un.Notification.CreatedAt,
            })
            .ToListAsync();

        return list;
    }

    public async Task<int> GetUnreadCountAsync(int userId, int conventionId)
    {
        return await _unitOfWork.UserNotifications.Query
            .Where(un => un.UserId == userId && !un.IsRead && un.Notification.ConventionId == conventionId)
            .CountAsync();
    }

    public async Task<bool> MarkReadAsync(int userNotificationId, int userId)
    {
        var un = await _unitOfWork.UserNotifications.GetByIdAsync(userNotificationId);
        if (un == null || un.UserId != userId) return false;
        un.IsRead = true;
        un.ReadAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllReadAsync(int userId, int conventionId)
    {
        var unread = await _unitOfWork.UserNotifications.Query
            .Where(un => un.UserId == userId && !un.IsRead && un.Notification.ConventionId == conventionId)
            .ToListAsync();

        foreach (var un in unread) { un.IsRead = true; un.ReadAt = DateTime.UtcNow; }
        await _unitOfWork.SaveChangesAsync();
        return unread.Count;
    }
}

public class SendNotificationRequest
{
    public string Type { get; set; } = "TEXT";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int? ReferenceId { get; set; }
    public string TargetScope { get; set; } = "ALL";
    public string? TargetGroupName { get; set; }
    public List<int>? TargetUserIds { get; set; }
}
