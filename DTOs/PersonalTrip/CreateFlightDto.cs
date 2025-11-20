using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 교통수단 생성/수정용 DTO
    /// </summary>
    public class CreateFlightDto
    {
        [MaxLength(20)]
        public string Category { get; set; } = "항공편";

        public int? ItineraryItemId { get; set; }

        public decimal? Amount { get; set; }

        public decimal? TollFee { get; set; }

        public decimal? FuelCost { get; set; }

        public decimal? ParkingFee { get; set; }

        public decimal? RentalCost { get; set; }

        [MaxLength(100)]
        public string? Airline { get; set; }

        [MaxLength(50)]
        public string? FlightNumber { get; set; }

        [MaxLength(200)]
        public string? DepartureLocation { get; set; }

        [MaxLength(200)]
        public string? ArrivalLocation { get; set; }

        public DateTime? DepartureTime { get; set; }

        public DateTime? ArrivalTime { get; set; }

        [MaxLength(100)]
        public string? BookingReference { get; set; }

        [MaxLength(20)]
        public string? SeatNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
