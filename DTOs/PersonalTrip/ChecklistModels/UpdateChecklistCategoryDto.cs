using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class UpdateChecklistCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
