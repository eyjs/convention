using LocalRAG.DTOs.UploadModels;

namespace LocalRAG.Interfaces;

public interface INameTagUploadService
{
    Task<NameTagUploadResult> UploadNameTagsAsync(Stream stream);
}
