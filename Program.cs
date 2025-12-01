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
using LocalRAG.Services.Flight; // Added
using LocalRAG.Services.Shared;
using LocalRAG.Services.Shared.Builders;
using LocalRAG.Services.Upload;
using LocalRAG.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using LocalRAG.Hubs;

var builder = WebApplication.CreateBuilder(args);

// HTTP와 HTTPS 모두 지원하도록 설정
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // HTTP 포트 (5000)
    serverOptions.ListenAnyIP(5000);

    // HTTPS 포트 (5001) - 인증서가 있는 경우에만 활성화
    // 프로덕션에서는 리버스 프록시(IIS, Nginx 등)가 HTTPS 처리
    // serverOptions.ListenAnyIP(5001, listenOptions =>
    // {
    //     listenOptions.UseHttps();
    // });
});

// --- 1. 로깅 설정 ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// --- 2. 컨트롤러, CORS, Swagger 등 기본 서비스 등록 ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// LLM Provider 전용 HttpClient (타임아웃 5분)
builder.Services.AddHttpClient("LlmClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

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
        policy.SetIsOriginAllowed(origin =>
        {
            // 설정된 허용 도메인 목록
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000" };

            // 정확히 일치하는 도메인 확인
            if (allowedOrigins.Contains(origin))
                return true;

            // Vercel 프리뷰 도메인 허용 (*.vercel.app)
            if (origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// --- 3. 데이터베이스 및 리포지토리 설정 ---
builder.Services.AddConnectionStringProvider();

// Connection string을 한 번만 가져와서 재사용
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection이 설정되지 않았습니다.");

// DbContextPool 등록 (일반 리포지토리용)
builder.Services.AddDbContextPool<ConventionDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

// DbContextFactory 등록 (MssqlVectorStore용)
builder.Services.AddPooledDbContextFactory<ConventionDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

builder.Services.AddRepositories();
if (builder.Configuration.GetValue<bool>("EmbeddingSettings:UseOnnx", false))
{
    builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
}
else
{
    builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
}

// Vector Store 등록 - MSSQL 사용
builder.Services.AddScoped<IVectorStore, MssqlVectorStore>();

builder.Services.AddScoped<ILlmProvider, Llama3Provider>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("LlmClient"); // 300초 클라이언트
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<Llama3Provider>>();
    return new Llama3Provider(httpClient, configuration, logger);
});

builder.Services.AddScoped<ILlmProvider, GeminiProvider>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("LlmClient"); 
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new GeminiProvider(httpClient, configuration);
});

// LlmProviderManager 등록 (DB 기반 동적 Provider 관리)
builder.Services.AddScoped<LlmProviderManager>();


// 핵심 서비스들을 'Scoped'로 등록합니다.
builder.Services.AddScoped<IRagService, RagService>();

// 기존 수동 서비스는 제거하거나 주석 처리
builder.Services.AddScoped<ConventionDocumentBuilder>();
builder.Services.AddScoped<IndexingService>();
builder.Services.AddScoped<IConventionChatService, ConventionChatService>();

builder.Services.AddScoped<SourceIdentifier>();
builder.Services.AddScoped<ChatIntentRouter>();
builder.Services.AddScoped<ChatPromptBuilder>();
builder.Services.AddScoped<LlmResponseService>();
builder.Services.AddScoped<RagSearchService>();
builder.Services.AddScoped<UserContextualDataProvider>();
builder.Services.AddScoped<ConventionAccessService>();

// --- 5. 인증 및 기타 서비스 등록 ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings(); // Moved declaration here

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/chathub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<INoticeCategoryService, NoticeCategoryService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IActionOrchestrationService, ActionOrchestrationService>();
builder.Services.AddScoped<IFormBuilderService, LocalRAG.Services.FormBuilder.FormBuilderService>();
builder.Services.AddScoped<IPersonalTripService, LocalRAG.Services.PersonalTrip.PersonalTripService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();

// 파일 업로드 서비스 등록
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IUserUploadService, UserUploadService>();
builder.Services.AddScoped<IScheduleTemplateUploadService, ScheduleUploadService>();
builder.Services.AddScoped<IAttributeUploadService, AttributeUploadService>();
builder.Services.AddScoped<IGroupScheduleMappingService, GroupScheduleMappingService>();
builder.Services.AddScoped<INameTagUploadService, NameTagUploadService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextFactory, UserContextFactory>();
builder.Services.AddHttpClient<IFlightService, FlightService>();

// HealthChecks
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

// // 개발 환경에서는 HTTPS 리다이렉션 비활성화 (현재 임시로 전체 주석 처리)
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

// 정적 파일 서비스 설정
app.UseStaticFiles(); // wwwroot 폴더

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