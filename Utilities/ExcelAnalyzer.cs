using OfficeOpenXml;
using System.Text;

namespace LocalRAG.Utilities;

/// <summary>
/// Excel 파일 구조 분석을 위한 유틸리티
/// </summary>
public class ExcelAnalyzer
{
    public static async Task<string> AnalyzeExcelFile(string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var sb = new StringBuilder();
        sb.AppendLine($"=== Analyzing: {Path.GetFileName(filePath)} ===\n");

        using var package = new ExcelPackage(new FileInfo(filePath));

        foreach (var worksheet in package.Workbook.Worksheets)
        {
            sb.AppendLine($"[Sheet: {worksheet.Name}]");

            if (worksheet.Dimension == null)
            {
                sb.AppendLine("  (Empty sheet)\n");
                continue;
            }

            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            sb.AppendLine($"  Rows: {rowCount}, Columns: {colCount}");

            // 헤더 분석 (첫 3행)
            sb.AppendLine("  Headers:");
            for (int row = 1; row <= Math.Min(3, rowCount); row++)
            {
                sb.Append($"    Row {row}: ");
                for (int col = 1; col <= Math.Min(10, colCount); col++)
                {
                    var cell = worksheet.Cells[row, col];
                    var value = cell.Text?.Trim();

                    // 엑셀 내부 줄바꿈 감지
                    var hasLineBreaks = cell.Value?.ToString()?.Contains("\n") ?? false;
                    var lineBreakIndicator = hasLineBreaks ? " [LF]" : "";

                    sb.Append($"[{col}]={value}{lineBreakIndicator} | ");
                }
                sb.AppendLine();
            }

            // 샘플 데이터 (4-6행)
            if (rowCount > 3)
            {
                sb.AppendLine("  Sample Data:");
                for (int row = 4; row <= Math.Min(6, rowCount); row++)
                {
                    sb.Append($"    Row {row}: ");
                    for (int col = 1; col <= Math.Min(10, colCount); col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        var value = cell.Text?.Trim();

                        if (!string.IsNullOrEmpty(value))
                        {
                            var displayValue = value.Length > 20 ? value.Substring(0, 20) + "..." : value;
                            sb.Append($"[{col}]={displayValue} | ");
                        }
                    }
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
