using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.Flight
{
    /// <summary>
    /// 인천공항 API 응답 데이터 저장용 Entity
    /// </summary>
    public class IncheonFlightData
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 항공편명 (예: KE123, OZ6889)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string FlightId { get; set; } = null!;

        /// <summary>
        /// 항공사
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Airline { get; set; } = null!;

        /// <summary>
        /// 공항명 (출발지 또는 도착지)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Airport { get; set; } = null!;

        /// <summary>
        /// 공항 코드 (IATA)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string AirportCode { get; set; } = null!;

        /// <summary>
        /// 운항 타입 (DEPARTURE, ARRIVAL)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string FlightType { get; set; } = null!;

        /// <summary>
        /// 운항 날짜 (YYYYMMDD)
        /// </summary>
        [Required]
        [MaxLength(8)]
        public string ScheduleDate { get; set; } = null!;

        /// <summary>
        /// 예정 시간 (HH:mm)
        /// </summary>
        [MaxLength(10)]
        public string? ScheduleTime { get; set; }

        /// <summary>
        /// 예상 시간 (HH:mm)
        /// </summary>
        [MaxLength(10)]
        public string? EstimatedTime { get; set; }

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
        /// 체크인 카운터
        /// </summary>
        [MaxLength(100)]
        public string? CheckInCounter { get; set; }

        /// <summary>
        /// 운항 상태 (예: 출발, 지연, 결항)
        /// </summary>
        [MaxLength(50)]
        public string? Status { get; set; }

        /// <summary>
        /// 마스터 편명 (코드쉐어 운항인 경우)
        /// </summary>
        [MaxLength(20)]
        public string? MasterFlightId { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 수정일
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // 인덱스를 위한 복합키 설정
        // FlightId + ScheduleDate + FlightType 조합으로 유니크하게 식별
    }
}
