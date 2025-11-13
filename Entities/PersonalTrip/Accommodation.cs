using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    /// <summary>
    /// 숙소 정보
    /// </summary>
    public class Accommodation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 소속 여행
        /// </summary>
        [Required]
        public int PersonalTripId { get; set; }

        /// <summary>
        /// 숙소명
        /// </summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// 숙소 타입 (호텔, 에어비앤비, 게스트하우스 등)
        /// </summary>
        [MaxLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// 주소
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// 우편번호
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// 위도
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 경도
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 체크인 일시
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// 체크아웃 일시
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// 예약 번호
        /// </summary>
        [MaxLength(100)]
        public string? BookingReference { get; set; }

        /// <summary>
        /// 연락처
        /// </summary>
        [MaxLength(50)]
        public string? ContactNumber { get; set; }

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
