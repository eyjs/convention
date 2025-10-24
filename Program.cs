using LocalRAG.Configuration;
using LocalRAG.Data;
using LocalRAG.HealthChecks;
using LocalRAG.Interfaces;
using LocalRAG.Middleware;
using LocalRAG.Providers;
using LocalRAG.Repositories;
using LocalRAG.Services.Ai;
using LocalRAG.Services.Auth;
using LocalRAG.Services.Chat;
using LocalRAG.Services.Convention;
using LocalRAG.Services.Guest;
using LocalRAG.Services.Shared;
using LocalRAG.Services.Shared.Builders;
using LocalRAG.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using LocalRAG.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- 1. 로깅 설정 ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// --- 2. 컨트롤러, CORS, Swagger 등 기본 서비스 등록 ---
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// 세션 추가
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});
builder.Services.AddRepositories();


// --- 4. [핵심 수정] RAG 및 AI 관련 서비스 등록 (올바른 수명 주기 설정) ---

// VectorStore와 EmbeddingService는 데이터를 메모리에 유지하거나 모델을 로드해야 하므로 'Singleton'으로 등록합니다.
//builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
if (builder.Configuration.GetValue<bool>("EmbeddingSettings:UseOnnx", false))
{
    builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
}
else
{
    builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
}

// Vector Store 등록
if (builder.Environment.IsProduction())
{
    builder.Services.AddScoped<IVectorStore, MssqlVectorStore>(); // 👈 MSSQL (Scoped)
    Console.WriteLine("Using MSSQL Vector Store for Production.");
}
else
{
    builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>(); // 개발용 InMemory
    Console.WriteLine("Using InMemory Vector Store for Development.");
}

// LLM Provider들은 상태를 유지할 필요 없으므로 'Scoped'로 등록합니다.
builder.Services.AddScoped<Llama3Provider>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<Llama3Provider>>();
    return new Llama3Provider(httpClient, configuration, logger);
});

builder.Services.AddScoped<GeminiProvider>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new GeminiProvider(httpClient, configuration);
});

// 👉 LlmProviderManager 등록 (DB 기반 동적 Provider 관리)
builder.Services.AddScoped<LlmProviderManager>();

// 👉 ILlmProvider를 Factory 패턴으로 주입
builder.Services.AddScoped<ILlmProvider>(provider =>
{
    var providerManager = provider.GetRequiredService<LlmProviderManager>();
    return providerManager.GetActiveProviderAsync().GetAwaiter().GetResult();
});

// 핵심 서비스들을 'Scoped'로 등록합니다.
builder.Services.AddScoped<IRagService, RagService>();

// 기존 수동 서비스는 제거하거나 주석 처리
builder.Services.AddScoped<ConventionDocumentBuilder>();
builder.Services.AddScoped<IndexingService>();
builder.Services.AddScoped<IConventionChatService, ConventionChatService>();

builder.Services.AddScoped<SourceIdentifier>();
builder.Services.AddScoped<ChatPromptBuilder>();
builder.Services.AddScoped<LlmResponseService>();
builder.Services.AddScoped<RagSearchService>();
builder.Services.AddScoped<GuestContextualDataProvider>();
builder.Services.AddScoped<ConventionAccessService>();

// --- 5. 인증 및 기타 서비스 등록 ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IAuthService, LocalRAG.Services.Auth.AuthService>();
builder.Services.AddScoped<INoticeService, LocalRAG.Services.Convention.NoticeService>();
builder.Services.AddScoped<INoticeCategoryService, LocalRAG.Services.Convention.NoticeCategoryService>();
builder.Services.AddScoped<ISurveyService, LocalRAG.Services.Convention.SurveyService>();
builder.Services.AddSingleton<ISmsService, LocalRAG.Services.Auth.SmsService>();
builder.Services.AddSingleton<IVerificationService, LocalRAG.Services.Auth.VerificationService>();

// 파일 업로드 서비스 등록
builder.Services.AddScoped<IFileUploadService, LocalRAG.Services.Shared.FileUploadService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LocalRAG.Interfaces.IUserContextFactory, LocalRAG.Services.Shared.UserContextFactory>();

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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
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

// 정적 파일 서비스 설정
app.UseStaticFiles(); // wwwroot 폴더

// 업로드된 파일 서비스 설정 (d:\home 폴더)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine("d:", "home")),
    RequestPath = "/uploads"
});

app.UseSession();  // 세션 미들웨어 추가

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHub<ChatHub>("/chathub");
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