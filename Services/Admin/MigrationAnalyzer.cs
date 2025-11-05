using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LocalRAG.Services.Admin;

/// <summary>
/// Migration의 위험도를 분석하고 데이터 손실 가능성을 검출하는 서비스
/// </summary>
public class MigrationAnalyzer
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<MigrationAnalyzer> _logger;

    public MigrationAnalyzer(ConventionDbContext context, ILogger<MigrationAnalyzer> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Migration의 상세 분석 결과
    /// </summary>
    public class MigrationAnalysisResult
    {
        public string MigrationId { get; set; } = string.Empty;
        public string MigrationName { get; set; } = string.Empty;
        public RiskLevel OverallRisk { get; set; }
        public List<RiskItem> Risks { get; set; } = new();
        public List<TableImpact> AffectedTables { get; set; } = new();
        public string? SqlScript { get; set; }
        public bool RequiresBackup { get; set; }
        public bool RequiresReview { get; set; }
        public string? Recommendation { get; set; }
    }

    public enum RiskLevel
    {
        Safe = 0,      // 안전 (새 테이블/컬럼 추가 등)
        Low = 1,       // 낮음 (인덱스 추가 등)
        Medium = 2,    // 중간 (Nullable 컬럼 → NOT NULL 등)
        High = 3,      // 높음 (컬럼 타입 변경 등)
        Critical = 4   // 매우 위험 (DROP COLUMN, DROP TABLE 등)
    }

    public class RiskItem
    {
        public RiskLevel Level { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public string Column { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string? Mitigation { get; set; }
    }

    public class TableImpact
    {
        public string TableName { get; set; } = string.Empty;
        public long CurrentRowCount { get; set; }
        public List<string> AffectedColumns { get; set; } = new();
        public string Operation { get; set; } = string.Empty;
    }

    /// <summary>
    /// 대기 중인 Migration들을 분석
    /// </summary>
    public async Task<List<MigrationAnalysisResult>> AnalyzePendingMigrationsAsync()
    {
        var results = new List<MigrationAnalysisResult>();

        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

            foreach (var migrationId in pendingMigrations)
            {
                var analysis = await AnalyzeMigrationAsync(migrationId, appliedMigrations.LastOrDefault());
                results.Add(analysis);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze pending migrations");
        }

        return results;
    }

    /// <summary>
    /// 특정 Migration 분석
    /// </summary>
    private async Task<MigrationAnalysisResult> AnalyzeMigrationAsync(string migrationId, string? fromMigration)
    {
        var result = new MigrationAnalysisResult
        {
            MigrationId = migrationId,
            MigrationName = ExtractMigrationName(migrationId)
        };

        try
        {
            // SQL 스크립트 생성 (실제로는 dotnet ef 명령이 필요하지만, 런타임에서 대략적으로 추정)
            // 실제 운영에서는 빌드 시 미리 생성하거나 별도 프로세스 필요
            result.SqlScript = await GenerateSqlScriptAsync(fromMigration, migrationId);

            // SQL 스크립트 분석하여 위험 요소 검출
            if (!string.IsNullOrEmpty(result.SqlScript))
            {
                AnalyzeSqlScript(result);
            }

            // 영향받는 테이블 분석
            await AnalyzeAffectedTablesAsync(result);

            // 전체 위험도 계산
            result.OverallRisk = CalculateOverallRisk(result.Risks);

            // 권장 사항 생성
            GenerateRecommendation(result);

            result.RequiresBackup = result.OverallRisk >= RiskLevel.Medium;
            result.RequiresReview = result.OverallRisk >= RiskLevel.High;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze migration {MigrationId}", migrationId);
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Medium,
                Type = "ANALYSIS_ERROR",
                Description = "Migration 분석 중 오류 발생",
                Impact = "수동 검토 필요",
                Mitigation = "SQL 스크립트를 직접 확인하세요."
            });
        }

        return result;
    }

    /// <summary>
    /// SQL 스크립트 생성 (간단한 버전)
    /// </summary>
    private async Task<string?> GenerateSqlScriptAsync(string? fromMigration, string toMigration)
    {
        try
        {
            // 실제로는 EF Core의 IMigrationsSqlGenerator를 사용해야 하지만
            // 런타임에서는 제한적이므로 간단한 메시지 반환
            return $"-- Migration: {toMigration}\n-- SQL 스크립트는 개발 환경에서 'dotnet ef migrations script' 명령으로 생성하세요.\n";
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// SQL 스크립트에서 위험 요소 검출
    /// </summary>
    private void AnalyzeSqlScript(MigrationAnalysisResult result)
    {
        if (string.IsNullOrEmpty(result.SqlScript))
            return;

        var sql = result.SqlScript.ToUpperInvariant();

        // 1. DROP COLUMN 검출
        var dropColumnMatches = Regex.Matches(sql, @"DROP\s+COLUMN\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
        foreach (Match match in dropColumnMatches)
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Critical,
                Type = "DROP_COLUMN",
                Column = match.Groups[1].Value,
                Description = $"컬럼 '{match.Groups[1].Value}' 삭제",
                Impact = "⚠️ 해당 컬럼의 모든 데이터가 영구 삭제됩니다.",
                Mitigation = "데이터를 별도 테이블에 백업하거나, 컬럼 이름 변경인 경우 Migration 코드를 수정하세요."
            });
        }

        // 2. DROP TABLE 검출
        var dropTableMatches = Regex.Matches(sql, @"DROP\s+TABLE\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
        foreach (Match match in dropTableMatches)
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Critical,
                Type = "DROP_TABLE",
                Table = match.Groups[1].Value,
                Description = $"테이블 '{match.Groups[1].Value}' 삭제",
                Impact = "⚠️⚠️ 테이블의 모든 데이터가 영구 삭제됩니다!",
                Mitigation = "반드시 전체 DB 백업을 수행하세요."
            });
        }

        // 3. ALTER COLUMN 검출
        var alterColumnMatches = Regex.Matches(sql, @"ALTER\s+COLUMN\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
        foreach (Match match in alterColumnMatches)
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.High,
                Type = "ALTER_COLUMN",
                Column = match.Groups[1].Value,
                Description = $"컬럼 '{match.Groups[1].Value}' 타입/제약 조건 변경",
                Impact = "데이터 타입 변환 실패 시 Migration 오류 또는 데이터 손실 가능",
                Mitigation = "기존 데이터가 새 타입/제약 조건과 호환되는지 확인하세요."
            });
        }

        // 4. ADD COLUMN NOT NULL 검출
        if (sql.Contains("ADD") && sql.Contains("NOT NULL") && !sql.Contains("DEFAULT"))
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Medium,
                Type = "ADD_NOT_NULL_COLUMN",
                Description = "NOT NULL 컬럼 추가 (기본값 없음)",
                Impact = "기존 행에 NULL 값이 설정되어 제약 조건 위반 가능",
                Mitigation = "DEFAULT 값을 지정하거나, Nullable로 변경하세요."
            });
        }

        // 5. CREATE TABLE 검출 (안전)
        var createTableMatches = Regex.Matches(sql, @"CREATE\s+TABLE\s+\[?(\w+)\]?", RegexOptions.IgnoreCase);
        if (createTableMatches.Count > 0 && result.Risks.Count == 0)
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Safe,
                Type = "CREATE_TABLE",
                Description = "새 테이블 생성",
                Impact = "기존 데이터에 영향 없음",
                Mitigation = null
            });
        }

        // 6. ADD COLUMN (Nullable) 검출 (안전)
        if (sql.Contains("ADD") && sql.Contains("NULL") && !sql.Contains("NOT NULL"))
        {
            result.Risks.Add(new RiskItem
            {
                Level = RiskLevel.Safe,
                Type = "ADD_NULLABLE_COLUMN",
                Description = "Nullable 컬럼 추가",
                Impact = "기존 데이터에 영향 없음 (NULL 값으로 채워짐)",
                Mitigation = null
            });
        }
    }

    /// <summary>
    /// 영향받는 테이블의 현재 데이터 분석
    /// </summary>
    private async Task AnalyzeAffectedTablesAsync(MigrationAnalysisResult result)
    {
        try
        {
            // 영향받는 테이블 목록 추출
            var affectedTables = result.Risks
                .Where(r => !string.IsNullOrEmpty(r.Table))
                .Select(r => r.Table)
                .Distinct()
                .ToList();

            foreach (var tableName in affectedTables)
            {
                var impact = new TableImpact
                {
                    TableName = tableName,
                    Operation = string.Join(", ", result.Risks.Where(r => r.Table == tableName).Select(r => r.Type))
                };

                // 테이블 행 개수 조회
                try
                {
                    var countQuery = $"SELECT COUNT(*) FROM [{tableName}]";
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    using var command = connection.CreateCommand();
                    command.CommandText = countQuery;
                    var count = await command.ExecuteScalarAsync();
                    impact.CurrentRowCount = Convert.ToInt64(count);

                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get row count for table {TableName}", tableName);
                    impact.CurrentRowCount = -1; // 알 수 없음
                }

                // 영향받는 컬럼 목록
                impact.AffectedColumns = result.Risks
                    .Where(r => r.Table == tableName && !string.IsNullOrEmpty(r.Column))
                    .Select(r => r.Column)
                    .Distinct()
                    .ToList();

                result.AffectedTables.Add(impact);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze affected tables");
        }
    }

    /// <summary>
    /// 전체 위험도 계산
    /// </summary>
    private RiskLevel CalculateOverallRisk(List<RiskItem> risks)
    {
        if (!risks.Any())
            return RiskLevel.Safe;

        return risks.Max(r => r.Level);
    }

    /// <summary>
    /// 권장 사항 생성
    /// </summary>
    private void GenerateRecommendation(MigrationAnalysisResult result)
    {
        switch (result.OverallRisk)
        {
            case RiskLevel.Critical:
                result.Recommendation = "⚠️⚠️ 매우 위험한 Migration입니다!\n" +
                    "1. 반드시 전체 DB 백업을 수행하세요.\n" +
                    "2. SQL 스크립트를 직접 검토하세요.\n" +
                    "3. 테스트 환경에서 먼저 실행하세요.\n" +
                    "4. 데이터 손실이 발생할 수 있으므로 신중히 진행하세요.";
                break;

            case RiskLevel.High:
                result.Recommendation = "⚠️ 주의가 필요한 Migration입니다.\n" +
                    "1. 영향받는 테이블을 백업하세요.\n" +
                    "2. 기존 데이터가 새 제약 조건과 호환되는지 확인하세요.\n" +
                    "3. 가능하면 테스트 환경에서 먼저 실행하세요.";
                break;

            case RiskLevel.Medium:
                result.Recommendation = "⚠️ 일부 주의가 필요합니다.\n" +
                    "1. 영향받는 테이블의 백업을 권장합니다.\n" +
                    "2. Migration 후 데이터를 확인하세요.";
                break;

            case RiskLevel.Low:
                result.Recommendation = "✅ 비교적 안전한 Migration입니다.\n" +
                    "인덱스 추가 등 데이터에 직접적인 영향은 없으나, Migration 후 정상 작동을 확인하세요.";
                break;

            case RiskLevel.Safe:
                result.Recommendation = "✅ 안전한 Migration입니다.\n" +
                    "새 테이블/컬럼 추가 등 기존 데이터에 영향이 없습니다. 안심하고 실행하세요.";
                break;
        }
    }

    /// <summary>
    /// Migration ID에서 이름 추출
    /// </summary>
    private string ExtractMigrationName(string migrationId)
    {
        // 예: "20251104120000_AddGroupNameToUsers" → "AddGroupNameToUsers"
        var parts = migrationId.Split('_', 2);
        return parts.Length > 1 ? parts[1] : migrationId;
    }
}
