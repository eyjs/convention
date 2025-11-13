using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    /// <summary>
    /// 개인 여행의 개별 일정 항목
    /// </summary>
    public class ItineraryItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// N일차 (e.g., 1, 2, 3...)
        /// </summary>
        [Required]
        public int DayNumber { get; set; }

        /// <summary>
        /// 장소명 (e.g., "에펠탑", "루브르 박물관")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// 주소
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// 위도
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 경도
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Google Place ID
        /// </summary>
        [MaxLength(300)]
        public string? GooglePlaceId { get; set; }

        /// <summary>
        /// 시작 시간
        /// </summary>
        public TimeOnly? StartTime { get; set; }

        /// <summary>
        /// 종료 시간
        /// </summary>
        public TimeOnly? EndTime { get; set; }

        // Foreign Key
        [Required]
        public int PersonalTripId { get; set; }

        // Navigation property
        [ForeignKey(nameof(PersonalTripId))]
        public virtual PersonalTrip PersonalTrip { get; set; } = null!;
    }
}
