using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 숙소 생성/수정용 DTO
    /// </summary>
    public class CreateAccommodationDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [MaxLength(100)]
        public string? BookingReference { get; set; }

        [MaxLength(50)]
        public string? ContactNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public decimal? ExpenseAmount { get; set; }

    }
}
