using LocalRAG.Data;
using LocalRAG.HealthChecks;
using LocalRAG.Interfaces;
using LocalRAG.Middleware;
using LocalRAG.Providers;
using LocalRAG.Services;
using LocalRAG.Storage;
using LocalRAG.Configuration;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. 로깅 설정 ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// --- 2. 컨트롤러, CORS, Swagger 등 기본 서비스 등록 ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// --- 3. 데이터베이스 및 리포지토리 설정 ---
builder.Services.AddConnectionStringProvider();
builder.Services.AddSingleton<AutoIndexingInterceptor>();
builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();
    var interceptor = serviceProvider.GetRequiredService<AutoIndexingInterceptor>();

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    })
    .AddInterceptors(interceptor);
});
builder.Services.AddRepositories();


// --- 4. [핵심 수정] RAG 및 AI 관련 서비스 등록 (올바른 수명 주기 설정) ---

// VectorStore와 EmbeddingService는 데이터를 메모리에 유지하거나 모델을 로드해야 하므로 'Singleton'으로 등록합니다.
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
if (builder.Configuration.GetValue<bool>("EmbeddingSettings:UseOnnx", false))
{
    builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
}
else
{
    builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
}

// LLM Provider들은 상태를 유지할 필요 없으므로 'Scoped'로 등록합니다.
builder.Services.AddScoped<Llama3Provider>();
builder.Services.AddScoped<GeminiProvider>();

// 설정에 따라 사용할 LLM Provider를 동적으로 주입합니다.
builder.Services.AddScoped<ILlmProvider>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var llmType = configuration["LlmSettings:Provider"];

    return llmType?.ToLower() switch
    {
        "llama3" => provider.GetRequiredService<Llama3Provider>(),
        "gemini" => provider.GetRequiredService<GeminiProvider>(),
        _ => provider.GetRequiredService<Llama3Provider>()
    };
});

// 핵심 서비스들을 'Scoped'로 등록합니다.
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<ConventionChatService>();
builder.Services.AddScoped<ConventionIndexingService>();
builder.Services.AddScoped<IScheduleUploadService, ScheduleUploadService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<IGalleryService, GalleryService>();

// --- 5. 인증 및 기타 서비스 등록 ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();


// --- 6. 상태 확인(Health Check) 설정 ---
builder.Services.AddHealthChecks()
    .AddCheck<LlmProviderHealthCheck>("llm_provider")
    .AddCheck<VectorStoreHealthCheck>("vector_store")
    .AddCheck<EmbeddingServiceHealthCheck>("embedding_service")
    .AddDbContextCheck<ConventionDbContext>("database");


// --- 애플리케이션 빌드 및 미들웨어 파이프라인 구성 ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// CORS를 HTTPS 리다이렉션보다 먼저 설정
app.UseCors("AllowSPA");

// 개발 환경에서는 HTTPS 리다이렉션 비활성화
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapFallback("/api/{**slug}", (HttpContext context) =>
    {
        context.Response.StatusCode = 404;
        return "API endpoint not found";
    });
}
else
{
    app.UseDefaultFiles();
    app.MapFallbackToFile("index.html");
}

app.Run();