using LocalRAG.Data;
using LocalRAG.HealthChecks;
using LocalRAG.Interfaces;
using LocalRAG.Middleware;
using LocalRAG.Providers;
using LocalRAG.Services;
using LocalRAG.Storage;
using LocalRAG.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "LocalRAG Convention Management API", 
        Version = "1.0.0",
        Description = $"A comprehensive convention management system with RAG capabilities (Environment: {builder.Environment.EnvironmentName})"
    });
});

// CORS 설정 (SPA 지원)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JWT Authentication 설정
var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? "your-secret-key-here-minimum-32-characters-long-for-production";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configuration binding
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));

// Smart Connection String Provider 등록
builder.Services.AddSmartConnectionString();

// HttpClient for LLM providers
builder.Services.AddHttpClient();

// Entity Framework with Smart Connection String
builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
    
    // Development 환경에서만 Sensitive Data Logging 활성화
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register core services
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();

// Embedding service selection
var useOnnxEmbedding = builder.Configuration.GetValue<bool>("EmbeddingSettings:UseOnnx", false);
if (useOnnxEmbedding)
{
    builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
}
else
{
    builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
}

builder.Services.AddScoped<IRagService, RagService>();

// Register LLM providers
builder.Services.AddScoped<Llama3Provider>();
builder.Services.AddScoped<GeminiProvider>();

// LLM provider factory
builder.Services.AddScoped<ILlmProvider>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var llmType = configuration["LlmSettings:Provider"];
    
    return llmType?.ToLower() switch
    {
        "llama3" => provider.GetRequiredService<Llama3Provider>(),
        "gemini" => provider.GetRequiredService<GeminiProvider>(),
        _ => provider.GetRequiredService<Llama3Provider>() // default
    };
});

// Health checks with Smart Connection
builder.Services.AddHealthChecks()
    .AddCheck<LlmProviderHealthCheck>("llm_provider", HealthStatus.Degraded, new[] { "llm" })
    .AddCheck<VectorStoreHealthCheck>("vector_store", HealthStatus.Degraded, new[] { "storage" })
    .AddCheck<EmbeddingServiceHealthCheck>("embedding_service", HealthStatus.Degraded, new[] { "embedding" })
    .AddDbContextCheck<ConventionDbContext>("database", HealthStatus.Degraded, new[] { "database" });

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 환경별 로깅 레벨 조정
if (builder.Environment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

var app = builder.Build();

// Environment-specific middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocalRAG Convention Management API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

// Custom middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Standard middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS (SPA 지원)
app.UseCors("AllowSPA");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Enhanced health checks endpoint with connection info
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        // Connection info 추가
        var connectionProvider = context.RequestServices.GetRequiredService<IConnectionStringProvider>();
        var connectionInfo = connectionProvider.GetConnectionInfo();
        
        var response = new
        {
            Status = report.Status.ToString(),
            Environment = app.Environment.EnvironmentName,
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            ConnectionInfo = new
            {
                connectionInfo.Environment,
                connectionInfo.ServerName,
                connectionInfo.DatabaseName,
                connectionInfo.IsLocalConnection,
                connectionInfo.ResolvedAt
            },
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Tags = entry.Value.Tags
            }),
            Timestamp = DateTime.Now
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Map controllers
app.MapControllers();

// SPA Configuration
if (app.Environment.IsDevelopment())
{
    // 개발 환경에서는 Vite dev 서버로 프록시
    app.MapFallback("/api/{**slug}", (HttpContext context) =>
    {
        context.Response.StatusCode = 404;
        return "API endpoint not found";
    });
    
    // 개발 환경 SPA 프록시 설정
    app.MapFallback(async context =>
    {
        // API 경로가 아닌 경우에만 index.html 반환
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            // 개발 환경에서는 Vite dev 서버 안내
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>iFA Convention - Development</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
        .container { max-width: 600px; margin: 0 auto; }
        .btn { background: #14b8a6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 개발 환경</h1>
        <p>프론트엔드 개발 서버가 별도로 실행되어야 합니다.</p>
        <p><strong>ClientApp</strong> 폴더에서 다음 명령어를 실행하세요:</p>
        <pre style='background: #f4f4f4; padding: 20px; text-align: left;'>
cd ClientApp
npm install
npm run dev
        </pre>
        <p>그 다음 <a href='http://localhost:3000' class='btn'>http://localhost:3000</a>에서 프론트엔드를 확인하세요.</p>
        <hr>
        <p>API는 현재 <a href='/swagger' class='btn'>Swagger UI</a>에서 테스트할 수 있습니다.</p>
    </div>
</body>
</html>");
        }
    });
}
else
{
    // 프로덕션 환경에서는 빌드된 정적 파일 서빙
    app.UseDefaultFiles();
    app.MapFallbackToFile("index.html");
}

// Enhanced default route with environment info
app.MapGet("/api", (IConnectionStringProvider connectionProvider) => new
{
    Application = "LocalRAG Convention Management System",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Status = "Running",
    ConnectionInfo = connectionProvider.GetConnectionInfo(),
    Endpoints = new
    {
        Api = "/api",
        Health = "/health",
        Swagger = app.Environment.IsDevelopment() ? "/swagger" : null,
        SystemInfo = "/api/system/info",
        DatabaseTest = "/api/DatabaseTest",
        Frontend = app.Environment.IsDevelopment() ? "http://localhost:3000" : "/"
    },
    Features = new
    {
        SwaggerEnabled = app.Environment.IsDevelopment(),
        DetailedErrors = app.Environment.IsDevelopment(),
        SensitiveDataLogging = app.Environment.IsDevelopment(),
        SPAIntegration = true
    }
});

// Environment info endpoint
app.MapGet("/api/environment", (IConnectionStringProvider connectionProvider) => new
{
    Environment = app.Environment.EnvironmentName,
    IsDevelopment = app.Environment.IsDevelopment(),
    IsProduction = app.Environment.IsProduction(),
    IsStaging = app.Environment.IsStaging(),
    ConnectionInfo = connectionProvider.GetConnectionInfo(),
    Configuration = new
    {
        LlmProvider = app.Configuration["LlmSettings:Provider"],
        UseOnnxEmbedding = app.Configuration.GetValue<bool>("EmbeddingSettings:UseOnnx"),
        MaxConcurrentRequests = app.Configuration.GetValue<int>("PerformanceSettings:MaxConcurrentRequests")
    }
});

// Database initialization with environment awareness
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ConventionDbContext>();
    var connectionProvider = scope.ServiceProvider.GetRequiredService<IConnectionStringProvider>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var connectionInfo = connectionProvider.GetConnectionInfo();
        logger.LogInformation("Initializing database: Environment={Environment}, Server={Server}, Database={Database}", 
            connectionInfo.Environment, connectionInfo.ServerName, connectionInfo.DatabaseName);
            
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Convention database initialized successfully.");
        
        // Development 환경에서만 연결 세부 정보 로깅
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Connection Details: {ConnectionInfo}", 
                System.Text.Json.JsonSerializer.Serialize(connectionInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        
        // Production에서는 애플리케이션 시작 중단
        if (app.Environment.IsProduction())
        {
            throw;
        }
    }
}

// Startup 로그
var startupConnectionProvider = app.Services.GetRequiredService<IConnectionStringProvider>();
var startupConnectionInfo = startupConnectionProvider.GetConnectionInfo();
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

startupLogger.LogInformation("=== LocalRAG Convention Management System Started ===");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("Database: {Server}/{Database} (Local: {IsLocal})", 
    startupConnectionInfo.ServerName, startupConnectionInfo.DatabaseName, startupConnectionInfo.IsLocalConnection);

if (app.Environment.IsDevelopment())
{
    startupLogger.LogInformation("Frontend Dev Server: http://localhost:3000");
    startupLogger.LogInformation("Backend API: {BackendUrl}", app.Urls.FirstOrDefault() ?? "http://localhost:5000");
}

app.Run();
