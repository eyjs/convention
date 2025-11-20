using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class UpdateChecklistItemDto
    {
        [Required]
        [MaxLength(200)]
        public string Task { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsChecked { get; set; }

        public int Order { get; set; }
    }
}
