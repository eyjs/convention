namespace LocalRAG.DTOs.PersonalTrip
{
    public class ItineraryItemDto
    {
        public int Id { get; set; }
        public int DayNumber { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GooglePlaceId { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int OrderNum { get; set; }
        public string? Notes { get; set; }
    }
}
