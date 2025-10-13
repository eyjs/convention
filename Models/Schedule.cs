using System;
using System.Collections.Generic;

namespace LocalRAG.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime ScheduleDate { get; set; }
        
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public string? Group { get; set; }

        public string? Description { get; set; }
        
        public int OrderNum { get; set; }

        // Navigation properties removed - using ScheduleTemplate instead
    }
}