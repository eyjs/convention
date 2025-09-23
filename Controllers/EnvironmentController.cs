using Microsoft.AspNetCore.Mvc;
using LocalRAG.Configuration;
using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentController : ControllerBase
{
    private readonly IConnectionStringProvider _connectionProvider;
    private readonly IWebHostEnvironment _environment;
    private readonly ConventionDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnvironmentController> _logger;

    public EnvironmentController(
        IConnectionStringProvider connectionProvider,
        IWebHostEnvironment environment,
        ConventionDbContext context,
        IConfiguration configuration,
        ILogger<EnvironmentController> logger)
    {
        _connectionProvider = connectionProvider;
        _environment = environment;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 현재 환경 및 연결 정보 조회
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetEnvironmentInfo()
    {
        var connectionInfo = _connectionProvider.GetConnectionInfo();
        
        var result = new
        {
            Environment = new
            {
                Name = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsProduction = _environment.IsProduction(),
                IsStaging = _environment.IsStaging(),
                ContentRootPath = _environment.ContentRootPath,
                WebRootPath = _environment.WebRootPath
            },
            Database = new
            {
                connectionInfo.ServerName,
                connectionInfo.DatabaseName,
                connectionInfo.IsLocalConnection,
                connectionInfo.ResolvedAt,
                ConnectionString = _environment.IsDevelopment() ? 
                    connectionInfo.ConnectionString : "[HIDDEN IN PRODUCTION]"
            },
            Configuration = new
            {
                LlmProvider = _configuration["LlmSettings:Provider"],
                UseOnnxEmbedding = _configuration.GetValue<bool>("EmbeddingSettings:UseOnnx"),
                MaxConcurrentRequests = _configuration.GetValue<int>("PerformanceSettings:MaxConcurrentRequests"),
                ConnectionTimeout = _configuration.GetValue<int>("DatabaseSettings:ConnectionTimeout"),
                UseRemoteForDevelopment = _configuration.GetValue<bool>("DatabaseSettings:UseRemoteForDevelopment")
            },
            Features = new
            {
                SwaggerEnabled = _environment.IsDevelopment(),
                DetailedErrorsEnabled = _environment.IsDevelopment(),
                SensitiveDataLoggingEnabled = _environment.IsDevelopment(),
                HttpsRedirectionEnabled = true
            },
            Timestamp = DateTime.Now
        };

        return Ok(result);
    }

    /// <summary>
    /// 데이터베이스 연결 상태 및 환경 정보 확인
    /// </summary>
    [HttpGet("database-status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var connectionInfo = _connectionProvider.GetConnectionInfo();
            var startTime = DateTime.Now;
            
            // 데이터베이스 연결 테스트
            var canConnect = await _context.Database.CanConnectAsync();
            var connectionTime = DateTime.Now - startTime;
            
            string dbVersion = "Unknown";
            Dictionary<string, int> tableCounts = new();
            
            if (canConnect)
            {
                try
                {
                    // SQL Server 버전 정보 조회
                    var versionQuery = "SELECT @@VERSION";
                    dbVersion = await _context.Database.SqlQueryRaw<string>($"SELECT @@VERSION as Value").FirstOrDefaultAsync() ?? "Unknown";
                    
                    // 각 테이블의 레코드 수 조회
                    tableCounts["Conventions"] = await _context.Conventions.CountAsync();
                    tableCounts["Guests"] = await _context.Guests.CountAsync();
                    tableCounts["Schedules"] = await _context.Schedules.CountAsync();
                    tableCounts["GuestAttributes"] = await _context.GuestAttributes.CountAsync();
                    tableCounts["Features"] = await _context.Features.CountAsync();
                    
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("데이터베이스 정보 조회 중 오류: {Error}", ex.Message);
                    dbVersion = $"Error: {ex.Message}";
                }
            }

            var result = new
            {
                Environment = _environment.EnvironmentName,
                Database = new
                {
                    connectionInfo.ServerName,
                    connectionInfo.DatabaseName,
                    connectionInfo.IsLocalConnection,
                    Status = canConnect ? "Connected" : "Disconnected",
                    ConnectionTime = connectionTime.TotalMilliseconds,
                    Version = dbVersion.Length > 100 ? dbVersion.Substring(0, 100) + "..." : dbVersion,
                    TableCounts = tableCounts
                },
                Configuration = new
                {
                    ConnectionTimeout = _configuration.GetValue<int>("DatabaseSettings:ConnectionTimeout"),
                    MaxRetryCount = _configuration.GetValue<int>("DatabaseSettings:MaxRetryCount"),
                    UseRemoteForDevelopment = _configuration.GetValue<bool>("DatabaseSettings:UseRemoteForDevelopment")
                },
                TestResults = new
                {
                    CanConnect = canConnect,
                    ResponseTime = connectionTime.TotalMilliseconds,
                    TestedAt = DateTime.Now
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터베이스 상태 확인 중 오류 발생");
            return StatusCode(500, new
            {
                Error = "Database status check failed",
                Message = ex.Message,
                Environment = _environment.EnvironmentName,
                Timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 환경 전환 시뮬레이션 (개발용)
    /// </summary>
    [HttpPost("simulate-environment")]
    public IActionResult SimulateEnvironment([FromQuery] string targetEnvironment, [FromQuery] bool useRemote = false)
    {
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new { Message = "This endpoint is only available in development environment" });
        }

        var connectionInfo = _connectionProvider.GetConnectionInfo();
        
        // 시뮬레이션 결과 생성
        var simulatedConnection = targetEnvironment.ToLower() switch
        {
            "production" => new
            {
                Environment = "Production",
                ServerName = "172.25.1.21",
                DatabaseName = "ifa",
                IsLocalConnection = false,
                ExpectedBehavior = "Uses remote production database, minimal logging, no Swagger"
            },
            "staging" => new
            {
                Environment = "Staging", 
                ServerName = "172.25.1.21",
                DatabaseName = "ifa",
                IsLocalConnection = false,
                ExpectedBehavior = "Uses remote staging database, moderate logging"
            },
            "development" => new
            {
                Environment = "Development",
                ServerName = useRemote ? "172.25.1.21" : "localhost",
                DatabaseName = useRemote ? "ifa" : "startour",
                IsLocalConnection = !useRemote,
                ExpectedBehavior = "Uses " + (useRemote ? "remote" : "local") + " database, full logging, Swagger enabled"
            },
            _ => null
        };

        if (simulatedConnection == null)
        {
            return BadRequest(new { Message = "Invalid target environment. Use: development, staging, or production" });
        }

        return Ok(new
        {
            Current = new
            {
                connectionInfo.Environment,
                connectionInfo.ServerName,
                connectionInfo.DatabaseName,
                connectionInfo.IsLocalConnection
            },
            Simulated = simulatedConnection,
            Instructions = new
            {
                PowerShell = $"$env:ASPNETCORE_ENVIRONMENT = '{targetEnvironment}'; dotnet run",
                CommandLine = $"set ASPNETCORE_ENVIRONMENT={targetEnvironment} && dotnet run",
                Script = targetEnvironment.ToLower() switch
                {
                    "development" => useRemote ? ".\\dev-run.ps1 -UseRemote" : ".\\dev-run.ps1",
                    "production" => ".\\prod-run.ps1",
                    _ => $"$env:ASPNETCORE_ENVIRONMENT = '{targetEnvironment}'; dotnet run"
                }
            }
        });
    }

    /// <summary>
    /// 설정 값들 조회 (개발 환경에서만)
    /// </summary>
    [HttpGet("configuration")]
    public IActionResult GetConfiguration()
    {
        if (_environment.IsProduction())
        {
            return Ok(new
            {
                Message = "Configuration details are hidden in production environment",
                Environment = "Production",
                BasicInfo = new
                {
                    LlmProvider = _configuration["LlmSettings:Provider"],
                    DatabaseTimeout = _configuration.GetValue<int>("DatabaseSettings:ConnectionTimeout")
                }
            });
        }

        // 개발/스테이징 환경에서는 상세 설정 표시
        var allConfigs = new Dictionary<string, object>();
        
        // 각 섹션별 설정 수집
        var sections = new[] { "DatabaseSettings", "LlmSettings", "EmbeddingSettings", "RagSettings", "PerformanceSettings" };
        
        foreach (var section in sections)
        {
            var sectionConfig = new Dictionary<string, object>();
            var configSection = _configuration.GetSection(section);
            
            foreach (var child in configSection.GetChildren())
            {
                sectionConfig[child.Key] = child.Value ?? "[NULL]";
            }
            
            allConfigs[section] = sectionConfig;
        }

        return Ok(new
        {
            Environment = _environment.EnvironmentName,
            Configurations = allConfigs,
            ConnectionStrings = new
            {
                DefaultConnection = _configuration.GetConnectionString("DefaultConnection")?[..20] + "...",
                RemoteConnection = _configuration.GetConnectionString("RemoteConnection")?[..20] + "..."
            },
            RetrievedAt = DateTime.Now
        });
    }
}
