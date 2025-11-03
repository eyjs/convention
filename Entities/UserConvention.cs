using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities
{
    /// <summary>
    /// User와 Convention 간의 다대다 매핑 테이블
    /// 특정 행사에 참석하는 사용자를 나타냅니다.
    /// </summary>
    public class UserConvention
    {
        /// <summary>
        /// 사용자 ID (FK)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 행사 ID (FK)
        /// </summary>
        [Required]
        public int ConventionId { get; set; }

        /// <summary>
        /// 그룹명 (일정 템플릿 일괄 배정용)
        /// 예: "A그룹", "VIP", "일반참석자" 등
        /// </summary>
        [MaxLength(100)]
        public string? GroupName { get; set; }

        /// <summary>
        /// 비회원 접근용 고유 토큰
        /// 회원이 아닌 사용자가 이 행사에 액세스할 때 사용
        /// </summary>
        [MaxLength(64)]
        public string? AccessToken { get; set; }

        /// <summary>
        /// 마지막으로 채팅을 읽은 시간
        /// </summary>
        public DateTime? LastChatReadTimestamp { get; set; }

        /// <summary>
        /// 비자 서류 첨부파일 ID (FileAttachment 테이블 FK)
        /// 해외 여행 행사에 필요
        /// </summary>
        public int? VisaDocumentAttachmentId { get; set; }

        /// <summary>
        /// 이 매핑이 생성된 날짜
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ConventionId")]
        public virtual Convention Convention { get; set; } = null!;
    }
}
