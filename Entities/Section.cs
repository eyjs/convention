namespace LocalRAG.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string? Title { get; set; }
        public string? Contents { get; set; }
        public int OrderNum { get; set; }
        public DateTime RegDtm { get; set; }
        public string DeleteYn { get; set; } = string.Empty;

        public Menu? Menu { get; set; }
    }
}