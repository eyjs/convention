namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 숙소 조회용 DTO
    /// </summary>
    public class AccommodationDto
    {
        public int Id { get; set; }
        public int PersonalTripId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? BookingReference { get; set; }
        public string? ContactNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
