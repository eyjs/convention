using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.DTOs.UploadModels
{
    public class NameTagUploadResult
    {
        public bool Success { get; set; }
        public List<NameTagEntryDto> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalCount { get; set; }
        public int SuccessCount => Data.Count;
    }
}
