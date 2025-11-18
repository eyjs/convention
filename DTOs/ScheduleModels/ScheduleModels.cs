using LocalRAG.Entities;


namespace LocalRAG.DTOs.ScheduleModels
{
    /// <summary>
    /// 일정 템플릿 (재사용 가능한 일정 마스터)
    /// </summary>
    public class ScheduleTemplate
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }
        
        /// <summary>
        /// 코스명 (A코스, B코스, VIP코스 등)
        /// </summary>
        public string CourseName { get; set; } = string.Empty;
        
        /// <summary>
        /// 템플릿 설명
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 정렬 순서
        /// </summary>
        public int OrderNum { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public LocalRAG.Entities.Convention? Convention { get; set; }
        public ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
        public ICollection<GuestScheduleTemplate> GuestScheduleTemplates { get; set; } = new List<GuestScheduleTemplate>();
    }
    
    /// <summary>
    /// 일정 항목 (실제 일정 데이터)
    /// </summary>
    public class ScheduleItem
    {
        public int Id { get; set; }
        public int ScheduleTemplateId { get; set; }
        
        /// <summary>
        /// 일정 날짜
        /// </summary>
        public DateTime ScheduleDate { get; set; }
        
        /// <summary>
        /// 시작 시간 (HH:mm)
        /// </summary>
        public string StartTime { get; set; } = string.Empty;
        
        /// <summary>
        /// 종료 시간 (HH:mm, 선택)
        /// </summary>
        public string? EndTime { get; set; }
        
        /// <summary>
        /// 일정 제목 (예: 선유도 방문)
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 일정 내용 (HTML 지원)
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// 장소
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// 위도 (Latitude)
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 경도 (Longitude)
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 정렬 순서
        /// </summary>
        public int OrderNum { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public ScheduleTemplate? ScheduleTemplate { get; set; }
    }
    
    /// <summary>
    /// User와 ScheduleTemplate 연결 (참석자별 코스 배정)
    /// </summary>
    public class GuestScheduleTemplate
    {
        /// <summary>
        /// 사용자 ID (FK)
        /// </summary>
        public int UserId { get; set; }

        public int ScheduleTemplateId { get; set; }

        /// <summary>
        /// 배정일
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public ScheduleTemplate? ScheduleTemplate { get; set; }
    }

    public class UserScheduleDto
    {
        public int UserId { get; set; }
        public int ScheduleTemplateId { get; set; }
    }
}
