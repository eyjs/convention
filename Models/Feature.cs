using System.ComponentModel.DataAnnotations;

namespace LocalRAG.Models
{
    public class Feature
    {
        public int Id { get; set; }

        [Required]
        public int ConventionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MenuName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string MenuUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = false;

        [MaxLength(500)]
        public string IconUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Convention? Convention { get; set; }
    }
}
