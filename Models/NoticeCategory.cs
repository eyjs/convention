using System;
using System.Collections.Generic;

namespace LocalRAG.Models
{
    public class NoticeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public int ConventionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Convention Convention { get; set; } = null!;
        public virtual ICollection<Notice> Notices { get; set; } = new List<Notice>();
    }
}
