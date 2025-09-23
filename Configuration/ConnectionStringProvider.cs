using Microsoft.Extensions.Options;

namespace LocalRAG.Configuration;

/// <summary>
/// 데이터베이스 설정을 위한 옵션 클래스
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";
    
    public bool UseRemoteForDevelopment { get; set; } = false;
    public int ConnectionTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public bool EnableDetailedLogging { get; set; } = false;
}

/// <summary>
/// 환경에 따른 연결 문자열을 자동으로 선택하는 서비스
/// </summary>
public interface IConnectionStringProvider
{
    string GetConnectionString();
    string GetCurrentEnvironment();
    ConnectionInfo GetConnectionInfo();
}

public class ConnectionInfo
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty; // "Development" 또는 "Operation"
    public bool IsLocalConnection { get; set; }
    public DateTime ResolvedAt { get; set; } = DateTime.Now;
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly DatabaseSettings _databaseSettings;
    private readonly ILogger<ConnectionStringProvider> _logger;

    public ConnectionStringProvider(
        IConfiguration configuration, 
        IWebHostEnvironment environment,
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<ConnectionStringProvider> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _databaseSettings = databaseSettings.Value;
        _logger = logger;
    }

    public string GetConnectionString()
    {
        var connectionInfo = GetConnectionInfo();
        
        _logger.LogInformation("Connection resolved: Environment={Environment}, ConnectionType={ConnectionType}, Server={Server}, Database={Database}", 
            connectionInfo.Environment, connectionInfo.ConnectionType, connectionInfo.ServerName, connectionInfo.DatabaseName);
            
        return connectionInfo.ConnectionString;
    }

    public string GetCurrentEnvironment()
    {
        return _environment.EnvironmentName;
    }

    public ConnectionInfo GetConnectionInfo()
    {
        var environmentName = _environment.EnvironmentName;
        string connectionString;
        string connectionType;
        bool isLocal;

        // 환경별 연결 문자열 선택 로직
        if (_environment.IsDevelopment())
        {
            // 개발 환경에서 운영 DB 사용 강제 옵션 확인
            if (_databaseSettings.UseRemoteForDevelopment)
            {
                connectionString = _configuration.GetConnectionString("OperationConnectionString") ?? 
                                 throw new InvalidOperationException("OperationConnectionString이 설정되지 않았습니다.");
                connectionType = "Operation (Remote)";
                isLocal = false;
                _logger.LogWarning("🌐 개발 환경에서 운영 DB 연결을 사용합니다: {Server}", ExtractServerFromConnectionString(connectionString));
            }
            else
            {
                connectionString = _configuration.GetConnectionString("DevelopmentConnectionString") ?? 
                                 throw new InvalidOperationException("DevelopmentConnectionString이 설정되지 않았습니다.");
                connectionType = "Development (Local)";
                isLocal = true;
                _logger.LogInformation("🏠 개발 환경에서 로컬 DB 연결을 사용합니다: {Server}", ExtractServerFromConnectionString(connectionString));
            }
        }
        else if (_environment.IsProduction())
        {
            connectionString = _configuration.GetConnectionString("OperationConnectionString") ?? 
                             throw new InvalidOperationException("Production 환경에 OperationConnectionString이 설정되지 않았습니다.");
            connectionType = "Operation (Production)";
            isLocal = false;
            _logger.LogInformation("🏭 운영 환경에서 운영 DB 연결을 사용합니다: {Server}", ExtractServerFromConnectionString(connectionString));
        }
        else if (_environment.IsStaging())
        {
            connectionString = _configuration.GetConnectionString("OperationConnectionString") ?? 
                             throw new InvalidOperationException("Staging 환경에 OperationConnectionString이 설정되지 않았습니다.");
            connectionType = "Operation (Staging)";
            isLocal = false;
            _logger.LogInformation("🧪 스테이징 환경에서 운영 DB 연결을 사용합니다: {Server}", ExtractServerFromConnectionString(connectionString));
        }
        else
        {
            // 기타 환경 (Testing, etc.)
            connectionString = _configuration.GetConnectionString("DevelopmentConnectionString") ?? 
                             throw new InvalidOperationException($"환경 '{environmentName}'에 대한 연결 문자열이 설정되지 않았습니다.");
            connectionType = "Development (Fallback)";
            isLocal = true;
            _logger.LogInformation("🔧 환경 '{Environment}'에서 개발 DB 연결을 사용합니다", environmentName);
        }

        // 연결 문자열에서 정보 추출
        var serverName = ExtractServerFromConnectionString(connectionString);
        var databaseName = ExtractDatabaseFromConnectionString(connectionString);

        return new ConnectionInfo
        {
            ConnectionString = connectionString,
            Environment = environmentName,
            DatabaseName = databaseName,
            ServerName = serverName,
            ConnectionType = connectionType,
            IsLocalConnection = isLocal,
            ResolvedAt = DateTime.Now
        };
    }

    private string ExtractServerFromConnectionString(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';');
            var serverPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
            return serverPart?.Split('=')[1]?.Trim() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string ExtractDatabaseFromConnectionString(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';');
            var databasePart = parts.FirstOrDefault(p => p.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase));
            return databasePart?.Split('=')[1]?.Trim() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
    
    private string ExtractUserIdFromConnectionString(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';');
            var userPart = parts.FirstOrDefault(p => p.Trim().StartsWith("User ID=", StringComparison.OrdinalIgnoreCase));
            return userPart?.Split('=')[1]?.Trim() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}

/// <summary>
/// 연결 문자열 관련 확장 메서드
/// </summary>
public static class ConnectionStringExtensions
{
    public static IServiceCollection AddSmartConnectionString(this IServiceCollection services)
    {
        services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();
        return services;
    }

    public static string GetSmartConnectionString(this IServiceProvider serviceProvider)
    {
        var provider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
        return provider.GetConnectionString();
    }
}
