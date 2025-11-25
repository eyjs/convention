using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    /// <summary>
    /// 교통수단 정보 (항공편, 기차, 버스, 택시, 자가용, 렌트카)
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
        /// 교통수단 카테고리 (항공편, 기차, 버스, 택시, 자가용, 렌트카)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Category { get; set; } = "항공편";

        /// <summary>
        /// 연결된 일정 항목 (통계 및 비용 추적용)
        /// </summary>
        public int? ItineraryItemId { get; set; }

        /// <summary>
        /// 항공사 / 버스회사 / 택시회사
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
        /// 터미널
        /// </summary>
        [MaxLength(50)]
        public string? Terminal { get; set; }

        /// <summary>
        /// 게이트
        /// </summary>
        [MaxLength(20)]
        public string? Gate { get; set; }

        /// <summary>
        /// 운항 상태 (예: 출발, 지연, 결항)
        /// </summary>
        [MaxLength(50)]
        public string? Status { get; set; }
        
        /// <summary>
        /// 출발 공항 코드 (IATA)
        /// </summary>
        [MaxLength(10)]
        public string? DepartureAirportCode { get; set; }

        /// <summary>
        /// 도착 공항 코드 (IATA)
        /// </summary>
        [MaxLength(10)]
        public string? ArrivalAirportCode { get; set; }

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
        /// 금액 (총액)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// 톨비 (자가용, 렌트카)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TollFee { get; set; }

        /// <summary>
        /// 유류비/주유비 (자가용, 렌트카)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FuelCost { get; set; }

        /// <summary>
        /// 주차비 (자가용, 렌트카)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ParkingFee { get; set; }

        /// <summary>
        /// 렌트비용 (렌트카)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RentalCost { get; set; }

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

        [ForeignKey(nameof(ItineraryItemId))]
        public virtual ItineraryItem? ItineraryItem { get; set; }
    }
}
