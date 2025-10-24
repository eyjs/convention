namespace LocalRAG.DTOs.NoticeModels
{
    public class NoticeCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateNoticeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int ConventionId { get; set; }
    }

    public class UpdateNoticeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
    }
}