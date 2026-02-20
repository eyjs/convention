using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities;

[Table("SmsTemplates")]
public class SmsTemplate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    public string TemplateContent { get; set; } = string.Empty;

    /// <summary>
    /// 등록 일시
    /// </summary>
    public DateTime RegDtm { get; set; } = DateTime.Now;

    /// <summary>
    /// 삭제 여부 ('1': 삭제, '2': 정상)
    /// </summary>
    [Required]
    [MaxLength(1)]
    public string DeleteYn { get; set; } = "2";
}