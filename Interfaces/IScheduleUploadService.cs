namespace LocalRAG.Interfaces;

public interface IScheduleUploadService
{
    Task<ScheduleUploadResult> UploadGuestsAndSchedulesAsync(int conventionId, Stream excelStream);
}

public class ScheduleUploadResult
{
    public bool Success { get; set; }
    public int TotalSchedules { get; set; }
    public int GuestsCreated { get; set; }
    public int ScheduleAssignments { get; set; }
    public List<string> Errors { get; set; } = new();
}
