namespace LocalRAG.Entities;

public class Notification
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Type { get; set; } = "TEXT"; // TEXT, NOTICE, SURVEY, SCHEDULE, SEAT, LINK
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int? ReferenceId { get; set; }
    public string TargetScope { get; set; } = "ALL"; // ALL, GROUP, INDIVIDUAL
    public string? TargetGroupName { get; set; }
    public int SentByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Convention Convention { get; set; } = null!;
    public virtual User SentByUser { get; set; } = null!;
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}

public class UserNotification
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
