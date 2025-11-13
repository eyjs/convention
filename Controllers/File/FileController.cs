using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Interfaces;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using LocalRAG.DTOs.UploadModels;

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

    [HttpGet("viewer/{year}/{dayOfYear}/{fileName}")]
#pragma warning disable CA1416 // Validate platform compatibility
    public IActionResult GetImage(string year, string dayOfYear, string fileName, [FromQuery] string? resize = null)
    {
        var basePath = _fileUploadService.GetUploadBasePath();
        var filePath = Path.Combine(basePath, year, dayOfYear, fileName);

        if (!global::System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        try
        {
            string extension = Path.GetExtension(filePath);
            string lowerExtension = extension.Replace(".", "").ToLower();

            using global::System.Drawing.Image origin = ExifRotate(global::System.Drawing.Image.FromFile(filePath));
            using global::System.Drawing.Image image = ResizeImageCommon(origin, resize);
            
            var stream = new MemoryStream();
            image.Save(stream, GetImageFormat(lowerExtension));
            stream.Position = 0;

            return File(stream, $"image/{lowerExtension}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image {FilePath}", filePath);
            // Return a placeholder or error image? For now, just return a server error.
            return StatusCode(500, "Failed to process image.");
        }
    }

    #region Image Processing Helpers

    private static Image ExifRotate(Image img)
    {
        if (!img.PropertyIdList.Contains(0x0112)) return img;

        var prop = img.GetPropertyItem(0x0112);
        if (prop == null || prop.Value == null) return img;
        int val = BitConverter.ToUInt16(prop.Value, 0);
        var rot = global::System.Drawing.RotateFlipType.RotateNoneFlipNone;

        if (val == 3 || val == 4)
            rot = global::System.Drawing.RotateFlipType.Rotate180FlipNone;
        else if (val == 5 || val == 6)
            rot = global::System.Drawing.RotateFlipType.Rotate90FlipNone;
        else if (val == 7 || val == 8)
            rot = global::System.Drawing.RotateFlipType.Rotate270FlipNone;

        if (val == 2 || val == 4 || val == 5 || val == 7)
            rot |= global::System.Drawing.RotateFlipType.RotateNoneFlipX;

        if (rot != RotateFlipType.RotateNoneFlipNone)
            img.RotateFlip(rot);

        return img;
    }

    private static Image ResizeImageCommon(Image origin, string? resize)
    {
        if (string.IsNullOrEmpty(resize)) return origin;

        int width, height;
        try
        {
            var parts = resize.Split('x');
            width = int.Parse(parts[0]);
            height = int.Parse(parts[1]);
        }
        catch
        {
            return origin; // Invalid resize format
        }

        if (width <= 0 || height <= 0) return origin;

        var newImage = new global::System.Drawing.Bitmap(width, height);
        using (var graphics = global::System.Drawing.Graphics.FromImage(newImage))
        {
            graphics.CompositingQuality = global::System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = global::System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = global::System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.DrawImage(origin, 0, 0, width, height);
        }

        return newImage;
    }

    private static ImageFormat GetImageFormat(string lowerExtension)
    {
        return lowerExtension switch
        {
            "jpg" or "jpeg" => global::System.Drawing.Imaging.ImageFormat.Jpeg,
            "png" => global::System.Drawing.Imaging.ImageFormat.Png,
            "gif" => global::System.Drawing.Imaging.ImageFormat.Gif,
            "bmp" => global::System.Drawing.Imaging.ImageFormat.Bmp,
            "tiff" => global::System.Drawing.Imaging.ImageFormat.Tiff,
            "ico" => global::System.Drawing.Imaging.ImageFormat.Icon,
            _ => global::System.Drawing.Imaging.ImageFormat.Jpeg,
        };
    }

    #endregion
#pragma warning restore CA1416 // Validate platform compatibility
}
