namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 교통수단 조회용 DTO
    /// </summary>
    public class FlightDto
    {
        public int Id { get; set; }
        public int PersonalTripId { get; set; }
        public string Category { get; set; } = "항공편";
        public int? ItineraryItemId { get; set; }
        public string? Airline { get; set; }
        public string? FlightNumber { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? BookingReference { get; set; }
        public string? SeatNumber { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TollFee { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? ParkingFee { get; set; }
        public decimal? RentalCost { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
