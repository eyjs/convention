using LocalRAG.Entities.Action;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities
{
    public class GuestActionStatus
    {
        [Key]
        public int Id { get; set; }

        public int GuestId { get; set; }
        public int ConventionActionId { get; set; }

        public bool IsComplete { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        public string? ResponseDataJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }

        [ForeignKey("ConventionActionId")]
        public ConventionAction ConventionAction { get; set; }
    }
}