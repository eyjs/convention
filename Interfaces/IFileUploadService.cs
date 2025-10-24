using Microsoft.AspNetCore.Http;

namespace LocalRAG.Interfaces;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadImageAsync(IFormFile file, string? dateFolder = null);
    Task<FileUploadResult> UploadFileAsync(IFormFile file, string? dateFolder = null);
    Task<FileUploadResult> UploadBase64ImageAsync(string base64Data, string fileName, string? dateFolder = null);
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string relativePath);
    string GetUploadBasePath();
}

public record FileUploadResult(
    string Url,
    string FileName,
    string RelativePath,
    long FileSize
);
