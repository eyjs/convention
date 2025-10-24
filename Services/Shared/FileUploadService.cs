using System.Text.RegularExpressions;

using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LocalRAG.Services.Shared;

public class FileUploadService : IFileUploadService
{
    private readonly string _uploadBasePath;
    private readonly string _baseUrl;
    private readonly ILogger<FileUploadService> _logger;
    private readonly IWebHostEnvironment _environment;

    public FileUploadService(
        IConfiguration configuration,
        ILogger<FileUploadService> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        
        // appsettings.json에서 업로드 경로 읽기 (기본값: D:\Home)
        _uploadBasePath = configuration["FileUpload:BasePath"] ?? "D:\\Home";
        _baseUrl = configuration["FileUpload:BaseUrl"] ?? "/uploads";
        
        // 디렉토리가 없으면 생성
        if (!Directory.Exists(_uploadBasePath))
        {
            Directory.CreateDirectory(_uploadBasePath);
            _logger.LogInformation("Upload directory created: {Path}", _uploadBasePath);
        }
    }

    /// <summary>
    /// 이미지 파일 업로드
    /// </summary>
    public async Task<FileUploadResult> UploadImageAsync(IFormFile file, string? dateFolder = null)
    {
        // 파일 검증
        ValidateImageFile(file);

        // 날짜 폴더 생성 (YYYY/1~365)
        var year = DateTime.Now.Year.ToString();
        var dayOfYear = DateTime.Now.DayOfYear.ToString();
        var folderPath = Path.Combine(year, dayOfYear);
        var uploadPath = Path.Combine(_uploadBasePath, folderPath);
        
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // 파일명 생성 (중복 방지)
        var fileName = GenerateUniqueFileName(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        // 파일 저장
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 상대 경로 및 URL 생성
        var relativePath = $"{year}/{dayOfYear}/{fileName}";
        var url = $"/api/file/viewer/{year}/{dayOfYear}/{fileName}";

        _logger.LogInformation("Image uploaded: {FileName} -> {Path}", file.FileName, filePath);

        return new FileUploadResult(
            Url: url,
            FileName: fileName,
            RelativePath: relativePath,
            FileSize: file.Length
        );
    }

    /// <summary>
    /// 일반 파일 업로드
    /// </summary>
    public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string? dateFolder = null)
    {
        // 파일 검증 (악성 파일 차단)
        ValidateFile(file);

        var year = DateTime.Now.Year.ToString();
        var dayOfYear = DateTime.Now.DayOfYear.ToString();
        var folderPath = Path.Combine(year, dayOfYear);
        var uploadPath = Path.Combine(_uploadBasePath, folderPath);
        
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileName = GenerateUniqueFileName(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"{year}/{dayOfYear}/{fileName}";
        var url = $"/api/file/viewer/{year}/{dayOfYear}/{fileName}";

        _logger.LogInformation("File uploaded: {FileName} -> {Path}", file.FileName, filePath);

        return new FileUploadResult(
            Url: url,
            FileName: fileName,
            RelativePath: relativePath,
            FileSize: file.Length
        );
    }

    /// <summary>
    /// Base64 이미지 업로드
    /// </summary>
    public async Task<FileUploadResult> UploadBase64ImageAsync(string base64Data, string fileName, string? dateFolder = null)
    {
        try
        {
            // Base64 데이터에서 헤더 제거 (data:image/png;base64, 같은 부분)
            var base64 = base64Data;
            if (base64.Contains(","))
            {
                base64 = base64.Split(',')[1];
            }

            // Base64 -> byte[]
            var fileBytes = Convert.FromBase64String(base64);

            // 파일 크기 검증
            if (fileBytes.Length > 10 * 1024 * 1024) // 10MB
            {
                throw new InvalidOperationException("파일 크기가 10MB를 초과합니다.");
            }

            var year = DateTime.Now.Year.ToString();
            var dayOfYear = DateTime.Now.DayOfYear.ToString();
            var folderPath = Path.Combine(year, dayOfYear);
            var uploadPath = Path.Combine(_uploadBasePath, folderPath);
            
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var newFileName = GenerateUniqueFileName(fileName);
            var filePath = Path.Combine(uploadPath, newFileName);

            // 파일 저장
            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            var relativePath = $"{year}/{dayOfYear}/{newFileName}";
            var url = $"/api/file/viewer/{year}/{dayOfYear}/{newFileName}";

            _logger.LogInformation("Base64 image uploaded: {FileName} -> {Path}", fileName, filePath);

            return new FileUploadResult(
                Url: url,
                FileName: newFileName,
                RelativePath: relativePath,
                FileSize: fileBytes.Length
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Base64 이미지 업로드 실패");
            throw;
        }
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            // URL에서 상대 경로 추출
            var relativePath = filePath.Replace(_baseUrl + "/", "");
            var fullPath = Path.Combine(_uploadBasePath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation("File deleted: {Path}", fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 삭제 실패: {Path}", filePath);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 파일 URL 생성
    /// </summary>
    public string GetFileUrl(string relativePath)
    {
        return $"{_baseUrl}/{relativePath}";
    }

    public string GetUploadBasePath()
    {
        return _uploadBasePath;
    }

    #region Private Methods

    private void ValidateImageFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("이미지 파일만 업로드 가능합니다.");
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB
        {
            throw new InvalidOperationException("이미지 파일 크기는 10MB를 초과할 수 없습니다.");
        }

        // MIME 타입 검증
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new InvalidOperationException("유효하지 않은 이미지 형식입니다.");
        }
    }

    private void ValidateFile(IFormFile file)
    {
        // 위험한 확장자 차단
        var blockedExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (blockedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("업로드할 수 없는 파일 형식입니다.");
        }

        if (file.Length > 50 * 1024 * 1024) // 50MB
        {
            throw new InvalidOperationException("파일 크기는 50MB를 초과할 수 없습니다.");
        }
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        // 파일명 안전하게 변환
        var fileName = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);

        // 특수문자 제거
        fileName = Regex.Replace(fileName, @"[^\w\-가-힣]", "_");

        // 타임스탬프 + GUID로 고유 파일명 생성
        var timestamp = DateTime.Now.ToString("HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

        return $"{fileName}_{timestamp}_{uniqueId}{extension}";
    }

    #endregion
}
