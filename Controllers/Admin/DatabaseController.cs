using LocalRAG.Data;
using LocalRAG.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin/database")]
[Authorize(Roles = "Admin")]
public class DatabaseController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<DatabaseController> _logger;
    private readonly MigrationAnalyzer _migrationAnalyzer;

    public DatabaseController(
        ConventionDbContext context,
        ILogger<DatabaseController> logger,
        MigrationAnalyzer migrationAnalyzer)
    {
        _context = context;
        _logger = logger;
        _migrationAnalyzer = migrationAnalyzer;
    }

    /// <summary>
    /// 데이터베이스 상태 확인
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var status = new
            {
                canConnect = false,
                databaseExists = false,
                pendingMigrations = new List<string>(),
                appliedMigrations = new List<string>(),
                needsMigration = false,
                error = (string?)null
            };

            // 1. DB 연결 테스트
            try
            {
                await _context.Database.CanConnectAsync();
                status = status with { canConnect = true };
                _logger.LogInformation("Database connection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed.");
                return Ok(status with { error = $"DB 연결 실패: {ex.Message}" });
            }

            // 2. 데이터베이스 존재 확인
            try
            {
                var dbExists = await _context.Database.CanConnectAsync();
                status = status with { databaseExists = dbExists };
            }
            catch
            {
                status = status with { databaseExists = false };
            }

            // 3. Migration 상태 확인
            try
            {
                var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();
                var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();

                status = status with
                {
                    pendingMigrations = pendingMigrations,
                    appliedMigrations = appliedMigrations,
                    needsMigration = pendingMigrations.Any()
                };

                _logger.LogInformation(
                    "Migration status - Applied: {Applied}, Pending: {Pending}",
                    appliedMigrations.Count,
                    pendingMigrations.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check migration status. Database might not exist yet.");
                status = status with
                {
                    needsMigration = true,
                    error = "데이터베이스가 초기화되지 않았거나 테이블이 없습니다."
                };
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database status.");
            return StatusCode(500, new { error = $"상태 확인 실패: {ex.Message}" });
        }
    }

    /// <summary>
    /// 데이터베이스 Migration 실행
    /// </summary>
    [HttpPost("migrate")]
    public async Task<IActionResult> MigrateDatabase([FromQuery] bool dryRun = false)
    {
        try
        {
            _logger.LogWarning("Database migration requested by admin. DryRun: {DryRun}", dryRun);

            // 1. 현재 상태 확인
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return BadRequest(new { error = "데이터베이스에 연결할 수 없습니다." });
            }

            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();
            var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();

            if (!pendingMigrations.Any())
            {
                return Ok(new
                {
                    success = true,
                    message = "적용할 Migration이 없습니다. 데이터베이스가 최신 상태입니다.",
                    appliedMigrations
                });
            }

            // 2. Dry Run 모드 (실제 실행 없이 확인만)
            if (dryRun)
            {
                _logger.LogInformation("Dry run mode - Migration will not be executed.");
                return Ok(new
                {
                    dryRun = true,
                    message = "실제 실행하지 않음 (Dry Run)",
                    pendingMigrations,
                    appliedMigrations,
                    warning = "실제 Migration을 실행하려면 dryRun=false로 요청하세요."
                });
            }

            // 3. 실제 Migration 실행
            _logger.LogWarning("Executing database migration. Pending migrations: {Count}", pendingMigrations.Count);

            var startTime = DateTime.UtcNow;
            await _context.Database.MigrateAsync();
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Database migration completed successfully in {Duration}ms", duration.TotalMilliseconds);

            var newAppliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();

            return Ok(new
            {
                success = true,
                message = "Migration이 성공적으로 완료되었습니다.",
                executedMigrations = pendingMigrations,
                totalAppliedMigrations = newAppliedMigrations.Count,
                duration = $"{duration.TotalSeconds:F2}초"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            return StatusCode(500, new
            {
                success = false,
                error = "Migration 실행 실패",
                detail = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// 데이터베이스 연결 테스트
    /// </summary>
    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                var connectionString = _context.Database.GetConnectionString();
                var serverVersion = _context.Database.ProviderName;

                return Ok(new
                {
                    success = true,
                    message = "데이터베이스 연결 성공",
                    connectionString = MaskConnectionString(connectionString),
                    provider = serverVersion
                });
            }

            return StatusCode(500, new { success = false, error = "데이터베이스에 연결할 수 없습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed.");
            return StatusCode(500, new
            {
                success = false,
                error = "연결 테스트 실패",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Migration 상세 분석 (위험도 평가)
    /// </summary>
    [HttpGet("analyze-migrations")]
    public async Task<IActionResult> AnalyzeMigrations()
    {
        try
        {
            _logger.LogInformation("Analyzing pending migrations...");

            var analyses = await _migrationAnalyzer.AnalyzePendingMigrationsAsync();

            if (!analyses.Any())
            {
                return Ok(new
                {
                    message = "분석할 Migration이 없습니다.",
                    analyses = new List<object>()
                });
            }

            return Ok(new
            {
                message = $"{analyses.Count}개의 Migration을 분석했습니다.",
                analyses = analyses.Select(a => new
                {
                    migrationId = a.MigrationId,
                    migrationName = a.MigrationName,
                    overallRisk = a.OverallRisk.ToString(),
                    riskLevel = (int)a.OverallRisk,
                    requiresBackup = a.RequiresBackup,
                    requiresReview = a.RequiresReview,
                    recommendation = a.Recommendation,
                    risks = a.Risks.Select(r => new
                    {
                        level = r.Level.ToString(),
                        levelValue = (int)r.Level,
                        type = r.Type,
                        table = r.Table,
                        column = r.Column,
                        description = r.Description,
                        impact = r.Impact,
                        mitigation = r.Mitigation
                    }),
                    affectedTables = a.AffectedTables.Select(t => new
                    {
                        tableName = t.TableName,
                        currentRowCount = t.CurrentRowCount,
                        affectedColumns = t.AffectedColumns,
                        operation = t.Operation
                    }),
                    sqlScript = a.SqlScript
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze migrations.");
            return StatusCode(500, new
            {
                error = "Migration 분석 실패",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Migration SQL 스크립트 생성 (다운로드)
    /// </summary>
    [HttpGet("generate-sql")]
    public async Task<IActionResult> GenerateMigrationSql()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

            if (!pendingMigrations.Any())
            {
                return Ok(new { message = "적용할 Migration이 없습니다." });
            }

            // SQL 스크립트 생성 로직
            // 실제로는 dotnet ef migrations script 명령의 결과를 반환해야 하지만
            // 런타임에서는 불가능하므로, 경고 메시지와 함께 Migration 목록 반환
            return Ok(new
            {
                message = "SQL 스크립트는 개발 환경에서 'dotnet ef migrations script' 명령으로 생성하세요.",
                pendingMigrations,
                command = "dotnet ef migrations script -o migration.sql"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate migration SQL.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// ConnectionString에서 비밀번호 마스킹
    /// </summary>
    private static string? MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return connectionString;

        // Password=xxx 부분을 마스킹
        return Regex.Replace(
            connectionString,
            @"Password=([^;]+)",
            "Password=****",
            RegexOptions.IgnoreCase
        );
    }
}
