using LocalRAG.Entities.Action;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities
{
    /// <summary>
    /// 사용자의 액션 완료 상태 추적
    /// </summary>
    public class UserActionStatus
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 사용자 ID (FK)
        /// </summary>
        public int UserId { get; set; }

        public int ConventionActionId { get; set; }

        public bool IsComplete { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        public string? ResponseDataJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ConventionActionId")]
        public ConventionAction ConventionAction { get; set; } = null!;
    }
}