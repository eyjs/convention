using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip
{
    /// <summary>
    /// 개인 여행 생성용 DTO
    /// </summary>
    public class CreatePersonalTripDto
    {
        [Required(ErrorMessage = "여행 제목은 필수입니다.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "시작일은 필수입니다.")]
        public string StartDate { get; set; } = string.Empty; // yyyy-MM-dd

        [Required(ErrorMessage = "종료일은 필수입니다.")]
        public string EndDate { get; set; } = string.Empty; // yyyy-MM-dd

        [MaxLength(100)]
        public string? Destination { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(2)]
        public string? CountryCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
