// Models/DTOs/ConventionIndexingData.cs (새 파일 생성)

using LocalRAG.Entities;
using System.Collections.Generic;

namespace LocalRAG.DTOs.AiModels;

/// <summary>
/// 색인 생성을 위해 ConventionDocumentBuilder에게 전달될 데이터 모음 DTO입니다.
/// </summary>
public class ConventionIndexingData
{
    public required Convention Convention { get; set; }
    public required List<Notice> Notices { get; set; }
    public List<LocalRAG.Entities.Action.ConventionAction> ConventionActions { get; set; } = new List<LocalRAG.Entities.Action.ConventionAction>();
    // 나중에 FAQ가 추가되면 여기에 한 줄만 추가하면 됩니다.
    // public required List<Faq> Faqs { get; set; }
}