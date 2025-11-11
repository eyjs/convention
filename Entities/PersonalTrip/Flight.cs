using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    /// <summary>
    /// 항공권 정보
    /// </summary>
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 소속 여행
        /// </summary>
        [Required]
        public int PersonalTripId { get; set; }

        /// <summary>
        /// 항공사
        /// </summary>
        [MaxLength(100)]
        public string? Airline { get; set; }

        /// <summary>
        /// 항공편명
        /// </summary>
        [MaxLength(50)]
        public string? FlightNumber { get; set; }

        /// <summary>
        /// 출발지
        /// </summary>
        [MaxLength(200)]
        public string? DepartureLocation { get; set; }

        /// <summary>
        /// 도착지
        /// </summary>
        [MaxLength(200)]
        public string? ArrivalLocation { get; set; }

        /// <summary>
        /// 출발 일시
        /// </summary>
        public DateTime? DepartureTime { get; set; }

        /// <summary>
        /// 도착 일시
        /// </summary>
        public DateTime? ArrivalTime { get; set; }

        /// <summary>
        /// 예약 번호
        /// </summary>
        [MaxLength(100)]
        public string? BookingReference { get; set; }

        /// <summary>
        /// 좌석 번호
        /// </summary>
        [MaxLength(20)]
        public string? SeatNumber { get; set; }

        /// <summary>
        /// 메모
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 수정일
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PersonalTripId))]
        public virtual PersonalTrip PersonalTrip { get; set; } = null!;
    }
}
