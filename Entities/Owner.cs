namespace LocalRAG.Entities
{
    public class Owner
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;

        public Convention? Convention { get; set; }
    }
}