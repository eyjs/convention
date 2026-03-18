namespace LocalRAG.DTOs.ScheduleModels;

public class OptionTourAdminDto
{
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string? EndTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CustomOptionId { get; set; }
    public string? Content { get; set; }
}

public class AddParticipantsDto
{
    public List<int> UserIds { get; set; } = new();
}
