using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities.PersonalTrip
{
    public class ChecklistCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonalTripId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public int Order { get; set; }

        [ForeignKey(nameof(PersonalTripId))]
        public virtual PersonalTrip PersonalTrip { get; set; } = null!;

        public virtual ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }
}
