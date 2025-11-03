namespace LocalRAG.DTOs.NoticeModels
{
    public class NoticeCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public int NoticeCount { get; set; }
    }

    public class CreateNoticeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int ConventionId { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateNoticeCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}