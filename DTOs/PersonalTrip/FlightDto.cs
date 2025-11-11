namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 항공권 조회용 DTO
    /// </summary>
    public class FlightDto
    {
        public int Id { get; set; }
        public int PersonalTripId { get; set; }
        public string? Airline { get; set; }
        public string? FlightNumber { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? BookingReference { get; set; }
        public string? SeatNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
