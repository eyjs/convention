using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities
{
    /// <summary>
    /// 사용자의 커스텀 속성 (key-value 쌍)
    /// 행사별로 다양한 사용자 메타데이터를 저장
    /// </summary>
    public class GuestAttribute
    {
        public int Id { get; set; }

        /// <summary>
        /// 사용자 ID (FK)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 행사 ID (FK, nullable — 기존 데이터 하위 호환)
        /// </summary>
        public int? ConventionId { get; set; }

        public string AttributeKey { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ConventionId")]
        public Convention? Convention { get; set; }
    }
}