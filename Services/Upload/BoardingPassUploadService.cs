using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace LocalRAG.Services.Upload;

public class BoardingPassUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BoardingPassUploadService> _logger;
    private readonly IFileUploadService _fileUploadService;

    public BoardingPassUploadService(
        IUnitOfWork unitOfWork,
        ILogger<BoardingPassUploadService> logger,
        IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// 탑승권 PDF ZIP 일괄 업로드
    /// ZIP 내 PDF 파일명으로 참석자 이름 매칭
    /// </summary>
    public async Task<BoardingPassUploadResult> UploadAsync(int conventionId, IFormFile zipFile)
    {
        var result = new BoardingPassUploadResult();

        // 1. 행사 참석자 조회
        var guests = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .Include(uc => uc.User)
            .Select(uc => new { uc.UserId, uc.User.Name })
            .ToListAsync();

        if (guests.Count == 0)
        {
            result.Errors.Add("이 행사에 참석자가 없습니다.");
            return result;
        }

        // 2. ZIP 파일 열기
        using var zipStream = zipFile.OpenReadStream();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        var pdfEntries = archive.Entries
            .Where(e => e.Length > 0 && Path.GetExtension(e.Name).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (pdfEntries.Count == 0)
        {
            result.Errors.Add("ZIP 파일에 PDF가 없습니다.");
            return result;
        }

        // 3. 파일명 → 참석자 매칭 (확장자 제외)
        var matchResults = new List<BoardingPassMatch>();

        foreach (var entry in pdfEntries)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name).Trim();
            var match = new BoardingPassMatch
            {
                FileName = entry.Name,
                MatchName = fileNameWithoutExt,
            };

            var candidates = guests.Where(g => g.Name == fileNameWithoutExt).ToList();
            if (candidates.Count == 0)
            {
                match.Error = $"'{fileNameWithoutExt}' 참석자를 찾을 수 없습니다.";
                result.FailedList.Add(match);
            }
            else if (candidates.Count > 1)
            {
                match.Error = $"'{fileNameWithoutExt}' 동명이인 {candidates.Count}명";
                result.FailedList.Add(match);
            }
            else
            {
                match.UserId = candidates[0].UserId;
                match.Matched = true;
                matchResults.Add(match);
            }
        }

        // 4. 매칭 성공한 것만 저장 (부분 성공 허용)
        if (matchResults.Count == 0)
        {
            result.Errors.Add("매칭된 참석자가 없습니다. PDF 파일명을 참석자 이름과 동일하게 설정해주세요.");
            return result;
        }

        try
        {
            var basePath = _fileUploadService.GetUploadBasePath();
            var boardingDir = Path.Combine(basePath, "boarding", conventionId.ToString());
            Directory.CreateDirectory(boardingDir);

            foreach (var match in matchResults)
            {
                var entry = pdfEntries.First(e => e.Name == match.FileName);
                var savedFileName = $"{match.UserId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.pdf";
                var savedPath = Path.Combine(boardingDir, savedFileName);

                // 기존 탑승권 파일 삭제
                var uc = await _unitOfWork.UserConventions.Query
                    .FirstAsync(x => x.UserId == match.UserId && x.ConventionId == conventionId);

                if (!string.IsNullOrEmpty(uc.BoardingPassUrl))
                {
                    var oldFileName = Path.GetFileName(uc.BoardingPassUrl);
                    var oldPath = Path.Combine(boardingDir, oldFileName);
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                using var entryStream = entry.Open();
                using var fileStream = File.Create(savedPath);
                await entryStream.CopyToAsync(fileStream);

                uc.BoardingPassUrl = $"/api/file/docs/boarding/{conventionId}/{savedFileName}";

                result.MatchedCount++;
            }

            await _unitOfWork.SaveChangesAsync();
            result.Success = true;
            result.TotalFiles = pdfEntries.Count;

            _logger.LogInformation(
                "Boarding pass upload completed: convention={ConventionId}, matched={Matched}/{Total}, failed={Failed}",
                conventionId, result.MatchedCount, pdfEntries.Count, result.FailedList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Boarding pass upload failed");
            result.Errors.Add($"저장 중 오류: {ex.Message}");
        }

        return result;
    }
}

public class BoardingPassMatch
{
    public string FileName { get; set; } = string.Empty;
    public string MatchName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public bool Matched { get; set; }
    public string? Error { get; set; }
}

public class BoardingPassUploadResult
{
    public bool Success { get; set; }
    public int TotalFiles { get; set; }
    public int MatchedCount { get; set; }
    public List<BoardingPassMatch> FailedList { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
