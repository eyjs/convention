using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Services;

namespace LocalRAG.Controllers.File;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<FileController> _logger;

    public FileController(
        IFileUploadService fileUploadService,
        ILogger<FileController> logger)
    {
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// 단일 이미지 업로드 (에디터용)
    /// </summary>
    [HttpPost("upload/image")]
    [RequestSizeLimit(10_485_760)] // 10MB
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string? dateFolder = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "파일이 없습니다." });

            // 이미지 파일 검증
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "이미지 파일만 업로드 가능합니다." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "파일 크기는 10MB를 초과할 수 없습니다." });

            var result = await _fileUploadService.UploadImageAsync(file, dateFolder);

            return Ok(new
            {
                success = true,
                url = result.Url,
                fileName = result.FileName,
                size = result.FileSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "이미지 업로드 실패");
            return StatusCode(500, new { message = "업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 다중 파일 업로드 (첨부파일용)
    /// </summary>
    [HttpPost("upload/files")]
    [RequestSizeLimit(52_428_800)] // 50MB
    public async Task<IActionResult> UploadFiles(List<IFormFile> files, [FromQuery] string? dateFolder = null)
    {
        try
        {
            if (files == null || !files.Any())
                return BadRequest(new { message = "파일이 없습니다." });

            var results = new List<object>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    if (file.Length > 50 * 1024 * 1024)
                    {
                        errors.Add($"{file.FileName}: 50MB 초과");
                        continue;
                    }

                    var result = await _fileUploadService.UploadFileAsync(file, dateFolder);
                    results.Add(new
                    {
                        success = true,
                        url = result.Url,
                        fileName = result.FileName,
                        originalName = file.FileName,
                        size = result.FileSize
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"파일 업로드 실패: {file.FileName}");
                    errors.Add($"{file.FileName}: 업로드 실패");
                }
            }

            return Ok(new
            {
                success = true,
                files = results,
                errors = errors.Any() ? errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 업로드 실패");
            return StatusCode(500, new { message = "업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// Base64 이미지 업로드
    /// </summary>
    [HttpPost("upload/base64")]
    public async Task<IActionResult> UploadBase64([FromBody] Base64UploadRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Base64Data))
                return BadRequest(new { message = "Base64 데이터가 없습니다." });

            var result = await _fileUploadService.UploadBase64ImageAsync(
                request.Base64Data, 
                request.FileName ?? "image.png",
                request.DateFolder
            );

            return Ok(new
            {
                success = true,
                url = result.Url,
                fileName = result.FileName,
                size = result.FileSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Base64 이미지 업로드 실패");
            return StatusCode(500, new { message = "업로드 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    [HttpDelete("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest(new { message = "파일 경로가 없습니다." });

            var deleted = await _fileUploadService.DeleteFileAsync(filePath);

            if (!deleted)
                return NotFound(new { message = "파일을 찾을 수 없습니다." });

            return Ok(new { success = true, message = "파일이 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 삭제 실패");
            return StatusCode(500, new { message = "삭제 중 오류가 발생했습니다." });
        }
    }
}

public record Base64UploadRequest(
    string Base64Data,
    string? FileName = null,
    string? DateFolder = null
);
