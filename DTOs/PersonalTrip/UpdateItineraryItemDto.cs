using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip
{
    public class UpdateItineraryItemDto
    {
        [Required]
        public int DayNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string LocationName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(300)]
        public string? GooglePlaceId { get; set; }

        public string? StartTime { get; set; } // "HH:mm" format

        public string? EndTime { get; set; } // "HH:mm" format
    }
}
