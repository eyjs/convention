using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO.Compression;

namespace LocalRAG.Services.Upload;

public class PassportUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PassportUploadService> _logger;
    private readonly IFileUploadService _fileUploadService;

    public PassportUploadService(
        IUnitOfWork unitOfWork,
        ILogger<PassportUploadService> logger,
        IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// 여권 엑셀 + ZIP 일괄 업로드 (All-or-Nothing)
    /// Step 1: 검증 → Step 2: 확정
    /// </summary>
    public async Task<PassportUploadResult> ValidateAndUploadAsync(
        int conventionId, IFormFile excelFile, IFormFile? zipFile)
    {
        var result = new PassportUploadResult();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // 1. 행사 참석자 조회
        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .Select(uc => new { uc.User.Id, uc.User.Name, uc.User.Phone })
            .ToListAsync();

        if (guests.Count == 0)
        {
            result.Errors.Add("이 행사에 참석자가 없습니다.");
            return result;
        }

        // 2. 엑셀 파싱
        var rows = new List<PassportRow>();
        using (var stream = excelFile.OpenReadStream())
        using (var package = new ExcelPackage(stream))
        {
            var sheet = package.Workbook.Worksheets[0];
            if (sheet?.Dimension == null)
            {
                result.Errors.Add("엑셀 시트가 비어있습니다.");
                return result;
            }

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                var name = sheet.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrEmpty(name)) continue;

                rows.Add(new PassportRow
                {
                    Row = row,
                    Name = name,
                    LastName = sheet.Cells[row, 2].Text?.Trim(),
                    FirstName = sheet.Cells[row, 3].Text?.Trim(),
                    ExpiryDate = sheet.Cells[row, 4].Text?.Trim(),
                    FileName = sheet.Cells[row, 5].Text?.Trim(),
                });
            }
        }

        if (rows.Count == 0)
        {
            result.Errors.Add("엑셀에 데이터가 없습니다.");
            return result;
        }

        // 3. 파일명 중복 체크
        var fileNames = rows.Where(r => !string.IsNullOrEmpty(r.FileName)).Select(r => r.FileName!).ToList();
        var duplicateFileNames = fileNames.GroupBy(f => f).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateFileNames.Count > 0)
        {
            foreach (var dup in duplicateFileNames)
            {
                result.Errors.Add($"엑셀 파일명 중복: '{dup}'");
            }
            return result;
        }

        // 4. ZIP 파일 내용 확인
        var zipEntries = new Dictionary<string, ZipArchiveEntry>(); // 파일명(확장자제외) -> entry
        ZipArchive? archive = null;
        Stream? zipStream = null;

        if (zipFile != null)
        {
            zipStream = zipFile.OpenReadStream();
            archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (entry.Length == 0) continue; // 폴더 스킵
                var nameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name);
                if (!string.IsNullOrEmpty(nameWithoutExt))
                {
                    zipEntries[nameWithoutExt] = entry;
                }
            }
        }

        // 5. 매칭 검증
        var matchResults = new List<PassportMatchResult>();

        foreach (var row in rows)
        {
            var match = new PassportMatchResult { Row = row.Row, Name = row.Name };

            // 이름으로 참석자 매칭
            var candidates = guests.Where(g => g.Name == row.Name).ToList();
            if (candidates.Count == 0)
            {
                match.Error = $"'{row.Name}' 참석자를 찾을 수 없습니다.";
                result.Errors.Add($"Row {row.Row}: {match.Error}");
            }
            else if (candidates.Count > 1)
            {
                match.Error = $"'{row.Name}' 동명이인 {candidates.Count}명 — 전화번호로 구분 필요";
                result.Errors.Add($"Row {row.Row}: {match.Error}");
            }
            else
            {
                match.UserId = candidates[0].Id;
                match.Matched = true;
            }

            // 파일명 매칭
            if (!string.IsNullOrEmpty(row.FileName) && zipFile != null)
            {
                if (!zipEntries.ContainsKey(row.FileName))
                {
                    match.Error = $"ZIP에서 파일 '{row.FileName}'을 찾을 수 없습니다.";
                    result.Errors.Add($"Row {row.Row}: {match.Error}");
                    match.Matched = false;
                }
            }

            matchResults.Add(match);
        }

        // 6. 에러가 있으면 DB 삽입 안 함
        if (result.Errors.Count > 0)
        {
            archive?.Dispose();
            zipStream?.Dispose();
            result.TotalRows = rows.Count;
            return result;
        }

        // 7. 검증 통과 → DB 업데이트
        try
        {
            var basePath = _fileUploadService.GetUploadBasePath();
            var passportDir = Path.Combine(basePath, "passport", conventionId.ToString());
            Directory.CreateDirectory(passportDir);

            foreach (var (row, match) in rows.Zip(matchResults))
            {
                if (!match.Matched || match.UserId == null) continue;

                var user = await _unitOfWork.Users.Query
                    .FirstAsync(u => u.Id == match.UserId.Value);

                // 텍스트 정보 업데이트
                if (!string.IsNullOrEmpty(row.LastName)) user.LastName = row.LastName;
                if (!string.IsNullOrEmpty(row.FirstName)) user.FirstName = row.FirstName;
                if (!string.IsNullOrEmpty(row.ExpiryDate))
                {
                    if (DateOnly.TryParse(row.ExpiryDate, out var expiry))
                        user.PassportExpiryDate = expiry;
                }

                // 이미지 파일 저장
                if (!string.IsNullOrEmpty(row.FileName) && zipEntries.TryGetValue(row.FileName, out var entry))
                {
                    var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                    var savedFileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                    var savedPath = Path.Combine(passportDir, savedFileName);

                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.Create(savedPath);
                    await entryStream.CopyToAsync(fileStream);

                    user.PassportImageUrl = $"/api/file/viewer/passport/{conventionId}/{savedFileName}";
                }

                user.UpdatedAt = DateTime.UtcNow;
                result.UpdatedCount++;
            }

            await _unitOfWork.SaveChangesAsync();
            result.Success = true;
            result.TotalRows = rows.Count;

            _logger.LogInformation(
                "Passport bulk upload completed: convention={ConventionId}, updated={Updated}/{Total}",
                conventionId, result.UpdatedCount, rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Passport bulk upload failed");
            result.Errors.Add($"저장 중 오류: {ex.Message}");
        }
        finally
        {
            archive?.Dispose();
            zipStream?.Dispose();
        }

        return result;
    }
}

public class PassportRow
{
    public int Row { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? FileName { get; set; }
}

public class PassportMatchResult
{
    public int Row { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public bool Matched { get; set; }
    public string? Error { get; set; }
}

public class PassportUploadResult
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int UpdatedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
