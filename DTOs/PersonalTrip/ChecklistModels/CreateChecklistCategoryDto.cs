using System.ComponentModel.DataAnnotations;

namespace LocalRAG.DTOs.PersonalTrip.ChecklistModels
{
    public class CreateChecklistCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
