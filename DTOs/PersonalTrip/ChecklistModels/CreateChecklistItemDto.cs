using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class CreateChecklistItemDto
    {
        [Required]
        public int ChecklistCategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Task { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
