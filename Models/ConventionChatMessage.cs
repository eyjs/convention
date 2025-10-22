using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Models
{
    /// <summary>
    /// 채팅 메시지 엔티티
    /// </summary>
    public class ConventionChatMessage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 메시지가 속한 행사 ID
        /// </summary>
        [Required]
        public int ConventionId { get; set; }

        [ForeignKey("ConventionId")]
        public virtual Convention Convention { get; set; } = null!;

        /// <summary>
        /// 작성자 Guest ID
        /// </summary>
        [Required]
        public int GuestId { get; set; }

        [ForeignKey("GuestId")]
        public virtual Guest Guest { get; set; } = null!;

        /// <summary>
        /// 작성자 이름 (표시용)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        /// <summary>
        /// 메시지 내용
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 관리자가 작성한 메시지인지 여부
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// 작성 시각
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
