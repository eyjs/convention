namespace LocalRAG.DTOs.UploadModels;

public record Base64UploadRequest(
    string Base64Data,
    string? FileName = null,
    string? DateFolder = null
);
