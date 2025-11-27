using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LocalRAG.DTOs.UploadModels;
using LocalRAG.Interfaces;
using OfficeOpenXml;

namespace LocalRAG.Services.Upload;

public class NameTagUploadService : INameTagUploadService
{
    private readonly ILogger<NameTagUploadService> _logger;

    public NameTagUploadService(ILogger<NameTagUploadService> logger)
    {
        _logger = logger;
        // EPPlus 라이선스 설정
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<NameTagUploadResult> UploadNameTagsAsync(Stream stream)
    {
        var result = new NameTagUploadResult();
        var nameTags = new List<NameTagEntryDto>();

        try
        {
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                result.Errors.Add("Excel 파일에 시트가 없습니다.");
                result.Success = false;
                return result;
            }

            // 첫 번째 행은 헤더로 간주하고 건너뜁니다 (2행부터 시작).
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                // B열 (2번째 열)에서 테이블 이름 가져오기
                var tableName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty;

                // C열부터 시작하여 2칸씩(이름, 직책) 데이터를 읽어옵니다.
                // 양식에 따라 최대 6명 (C~N열)까지 읽습니다.
                for (int i = 0; i < 6; i++)
                {
                    int nameCol = 3 + (i * 2);
                    int titleCol = 4 + (i * 2);

                    var name = worksheet.Cells[row, nameCol].Value?.ToString()?.Trim();
                    var title = worksheet.Cells[row, titleCol].Value?.ToString()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        nameTags.Add(new NameTagEntryDto
                        {
                            Table = tableName,
                            Name = name,
                            Title = title
                        });
                    }
                }
            }

            result.Data = nameTags;
            result.TotalCount = nameTags.Count;
            result.Success = true;
            _logger.LogInformation("{Count}개의 명찰 데이터를 성공적으로 파싱했습니다.", nameTags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "명찰 데이터 파싱 중 오류가 발생했습니다.");
            result.Errors.Add($"서버 오류: {ex.Message}");
            result.Success = false;
        }

        return await Task.FromResult(result);
    }
}
