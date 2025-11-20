namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public int ChecklistCategoryId { get; set; }
        public string Task { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsChecked { get; set; }
        public int Order { get; set; }
    }
}
