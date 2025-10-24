using System.Collections.Generic;

namespace LocalRAG.Entities
{
    public class Menu
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }
        public string? ItemName { get; set; }
        public DateTime RegDtm { get; set; }
        public string DeleteYn { get; set; } = string.Empty;
        public int OrderNum { get; set; }

        public Convention? Convention { get; set; }
        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}