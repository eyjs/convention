namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class ChecklistCategoryDto
    {
        public int Id { get; set; }
        public int PersonalTripId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int Order { get; set; }
        public int CompletedItemsCount { get; set; }
        public int TotalItemsCount { get; set; }
        public List<ChecklistItemDto> Items { get; set; } = new();
    }
}
