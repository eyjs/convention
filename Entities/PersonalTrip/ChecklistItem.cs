using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    public class ChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChecklistCategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Task { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsChecked { get; set; } = false;

        public int Order { get; set; }

        [ForeignKey(nameof(ChecklistCategoryId))]
        public virtual ChecklistCategory Category { get; set; } = null!;
    }
}
