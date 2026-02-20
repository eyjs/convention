using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities;

[Table("SmsLogs")]
public class SmsLog
{
    /// <summary>
    /// 내부 발송 ID (PK) - snd_msg_id 역할
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 행사 ID (어떤 행사에서 보냈는지)
    /// 시스템 알림(인증번호 등)인 경우 null일 수 있음
    /// </summary>
    public int? ConventionId { get; set; }

    /// <summary>
    /// 수신자 이름 (rcv_name)
    /// </summary>
    [MaxLength(50)]
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// 수신자 연락처 (rcv_tel)
    /// </summary>
    [MaxLength(20)]
    public string ReceiverPhone { get; set; } = string.Empty;

    /// <summary>
    /// 발송 메시지 내용 (snd_msg)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 외부 프로시저 반환 ID (cmp_msg_id)
    /// 이 ID를 통해 추후 성공/실패 여부를 추적
    /// </summary>
    [MaxLength(100)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// 발송 시각
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey("ConventionId")]
    public virtual Convention? Convention { get; set; }
}
