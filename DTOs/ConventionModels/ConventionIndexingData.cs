using LocalRAG.Entities;
using System.Collections.Generic;

namespace LocalRAG.DTOs.ConventionModels
{
    public class ConventionIndexingData
    {
        public Convention Convention { get; set; }
        public Notice Notices { get; set; }
        // 필요한 경우 다른 관련 엔티티나 DTO를 추가할 수 있습니다.
    }
}