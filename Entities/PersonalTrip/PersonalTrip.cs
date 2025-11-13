using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    /// <summary>
    /// 개인 여행 정보
    /// </summary>
    public class PersonalTrip
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 여행 제목
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 여행 설명
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// 여행 시작일
        /// </summary>
        [Required]
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// 여행 종료일
        /// </summary>
        [Required]
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// 목적지 (국가)
        /// </summary>
        [MaxLength(100)]
        public string? Destination { get; set; }

        /// <summary>
        /// 목적지 도시
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// 국가 코드 (ISO 3166-1 alpha-2, 예: KR, JP, US)
        /// </summary>
        [MaxLength(2)]
        public string? CountryCode { get; set; }

        /// <summary>
        /// 목적지 위도
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 목적지 경도
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 소유자 (사용자 ID)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 수정일
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public virtual ICollection<ItineraryItem> ItineraryItems { get; set; } = new List<ItineraryItem>();
    }
}
