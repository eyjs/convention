using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocalRAG.Data;
using LocalRAG.Models;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadController> _logger;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".hwp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadController(
        ConventionDbContext context,
        IWebHostEnvironment environment,
        ILogger<UploadController> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 파일 업로드
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<FileUploadResponse>> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string category = "notice")
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "파일이 없습니다." });

            // 파일 크기 검증
            if (file.Length > MaxFileSize)
                return BadRequest(new { message = $"파일 크기는 {MaxFileSize / 1024 / 1024}MB 이하여야 합니다." });

            // 확장자 검증
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = AllowedImageExtensions.Concat(AllowedDocumentExtensions).ToArray();
            
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "지원하지 않는 파일 형식입니다." });

            // 업로드 디렉토리 생성
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", category);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // 파일명 생성 (GUID)
            var savedName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, savedName);

            // 파일 저장
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // DB에 메타데이터 저장
            var fileAttachment = new FileAttachment
            {
                OriginalName = file.FileName,
                SavedName = savedName,
                FilePath = $"/uploads/{category}/{savedName}",
                Size = file.Length,
                ContentType = file.ContentType,
                Category = category,
                UploadedAt = DateTime.Now
            };

            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();

            return Ok(new FileUploadResponse
            {
                Id = fileAttachment.Id,
                Url = fileAttachment.FilePath,
                OriginalName = fileAttachment.OriginalName,
                Size = fileAttachment.Size,
                ContentType = fileAttachment.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 업로드 실패");
            return StatusCode(500, new { message = "파일 업로드에 실패했습니다." });
        }
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        try
        {
            var file = await _context.FileAttachments.FindAsync(id);
            if (file == null)
                return NotFound(new { message = "파일을 찾을 수 없습니다." });

            // 물리적 파일 삭제
            var filePath = Path.Combine(_environment.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // DB에서 삭제
            _context.FileAttachments.Remove(file);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 삭제 실패: {Id}", id);
            return StatusCode(500, new { message = "파일 삭제에 실패했습니다." });
        }
    }

    /// <summary>
    /// 파일 다운로드
    /// </summary>
    [HttpGet("{savedName}")]
    public IActionResult DownloadFile(string savedName, [FromQuery] string category = "notice")
    {
        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", category, savedName);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "파일을 찾을 수 없습니다." });

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var file = _context.FileAttachments.FirstOrDefault(f => f.SavedName == savedName);
            
            var contentType = file?.ContentType ?? "application/octet-stream";
            var fileName = file?.OriginalName ?? savedName;

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 다운로드 실패: {SavedName}", savedName);
            return StatusCode(500, new { message = "파일 다운로드에 실패했습니다." });
        }
    }
}
