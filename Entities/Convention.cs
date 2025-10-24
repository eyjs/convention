using LocalRAG.DTOs.ScheduleModels;
using System;
using System.Collections.Generic;

namespace LocalRAG.Entities
{
    public class Convention
    {
        public int Id { get; set; }


        public string MemberId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string ConventionType { get; set; } = string.Empty;
        
        public string RenderType { get; set; } = "STANDARD";

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        
        public string? ConventionImg { get; set; }
        
        public string? BrandColor { get; set; }
        public string? ThemePreset { get; set; }
        
        public DateTime RegDtm { get; set; }
        public string DeleteYn { get; set; } = string.Empty;
        public string CompleteYn { get; set; } = "N";

        public ICollection<Guest> Guests { get; set; } = new List<Guest>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<ScheduleTemplate> ScheduleTemplates { get; set; } = new List<ScheduleTemplate>();
        public ICollection<Menu> Menus { get; set; } = new List<Menu>();
        public ICollection<Owner> Owners { get; set; } = new List<Owner>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
        public ICollection<VectorStore> VectorStores { get; set; } = new List<VectorStore>();
    }
}
