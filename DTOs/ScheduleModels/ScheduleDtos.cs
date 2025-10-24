namespace LocalRAG.DTOs.ScheduleModels;

public class GuestScheduleDto
{
    public int GuestId { get; set; }
    public int ScheduleTemplateId { get; set; }
}

public class ScheduleItemDto
{
    public int Id { get; set; }
    public int ScheduleTemplateId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string? EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Content { get; set; }
    public int OrderNum { get; set; }
    public string? CourseName { get; set; }
    public int ParticipantCount { get; set; }
}

public class ScheduleTemplateDto
{
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderNum { get; set; }
}

public class BulkScheduleItemsDto
{
    public List<ScheduleItemDto> Items { get; set; } = new();
}

public class AssignSchedulesDto
{
    public List<int> ScheduleTemplateIds { get; set; } = new();
}
