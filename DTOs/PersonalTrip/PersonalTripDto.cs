namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 개인 여행 조회용 DTO
    /// </summary>
    public class PersonalTripDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string StartDate { get; set; } = string.Empty; // yyyy-MM-dd
        public string EndDate { get; set; } = string.Empty; // yyyy-MM-dd
        public string? Destination { get; set; }
        public string? City { get; set; }
        public string? CountryCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<FlightDto> Flights { get; set; } = new();
        public List<AccommodationDto> Accommodations { get; set; } = new();
        public List<ItineraryItemDto> ItineraryItems { get; set; } = new();
    }
}
