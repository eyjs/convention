namespace LocalRAG.Models.DTOs
{
    public class NoticeCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateNoticeCategoryDto
    {
        public string Name { get; set; }
        public int ConventionId { get; set; }
    }

    public class UpdateNoticeCategoryDto
    {
        public string Name { get; set; }
    }
}
