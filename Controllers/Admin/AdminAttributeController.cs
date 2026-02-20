using LocalRAG.Data;
using LocalRAG.Constants;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminAttributeController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public AdminAttributeController(ConventionDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    [HttpGet("conventions/{conventionId}/attributes/keys")]
    public async Task<IActionResult> GetAttributeKeys(int conventionId)
    {
        var keys = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .SelectMany(uc => uc.User.GuestAttributes)
            .Select(ga => ga.AttributeKey)
            .Distinct()
            .OrderBy(k => k)
            .ToListAsync();

        return Ok(keys);
    }

    [HttpDelete("guests/{guestId}/attributes/{attributeKey}")]
    public async Task<IActionResult> DeleteGuestAttribute(int guestId, string attributeKey)
    {
        var decodedKey = Uri.UnescapeDataString(attributeKey);

        var attribute = await _context.GuestAttributes
            .FirstOrDefaultAsync(ga => ga.UserId == guestId && ga.AttributeKey == decodedKey);

        if (attribute == null)
            return NotFound(new { message = $"속성 '{decodedKey}'을 찾을 수 없습니다." });

        _context.GuestAttributes.Remove(attribute);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"속성 '{decodedKey}'이 삭제되었습니다." });
    }

    [HttpGet("conventions/{conventionId}/guests/download")]
    public async Task<IActionResult> DownloadGuestsForAttributeUpload(int conventionId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var guests = await _context.UserConventions
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
                .ThenInclude(u => u.GuestAttributes)
            .OrderBy(uc => uc.User.Name)
            .ToListAsync();

        if (!guests.Any())
            return NotFound(new { message = "참석자가 없습니다." });

        var allAttributeKeys = guests
            .SelectMany(uc => uc.User.GuestAttributes.Select(ga => ga.AttributeKey))
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("참석자속성");

        WriteExcelHeader(sheet, allAttributeKeys);
        WriteExcelData(sheet, guests, allAttributeKeys);

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        var fileBytes = package.GetAsByteArray();
        var fileName = $"참석자속성_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

        return File(fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost("conventions/upload-cover-image")]
    public async Task<IActionResult> UploadConventionCoverImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "파일이 제공되지 않았습니다." });

        try
        {
            var uploadResult = await _fileUploadService.UploadImageAsync(file, "convention-covers");

            if (string.IsNullOrEmpty(uploadResult.Url))
                return StatusCode(500, new { message = "커버 이미지 업로드에 실패했습니다." });

            return Ok(new { url = uploadResult.Url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static void WriteExcelHeader(ExcelWorksheet sheet, List<string> attributeKeys)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "이름";
        sheet.Cells[1, 3].Value = "전화번호";

        for (int i = 0; i < attributeKeys.Count; i++)
            sheet.Cells[1, 4 + i].Value = attributeKeys[i];

        using var headerRange = sheet.Cells[1, 1, 1, 3 + attributeKeys.Count];
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(255, 211, 211, 211);
    }

    private static void WriteExcelData(
        ExcelWorksheet sheet,
        List<UserConvention> guests,
        List<string> attributeKeys)
    {
        int row = 2;
        foreach (var uc in guests)
        {
            sheet.Cells[row, 1].Value = uc.UserId;
            sheet.Cells[row, 2].Value = uc.User.Name;
            sheet.Cells[row, 3].Value = uc.User.Phone;

            for (int i = 0; i < attributeKeys.Count; i++)
            {
                var attribute = uc.User.GuestAttributes
                    .FirstOrDefault(ga => ga.AttributeKey == attributeKeys[i]);
                if (attribute != null)
                    sheet.Cells[row, 4 + i].Value = attribute.AttributeValue;
            }

            row++;
        }
    }
}
