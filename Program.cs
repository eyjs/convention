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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddConnectionStringProvider();
builder.Services.AddHttpClient();

// Auto-indexing interceptor 등록
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
    .AddInterceptors(interceptor); // 인터셉터 추가
});

builder.Services.AddRepositories();
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();

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
builder.Services.AddScoped<Llama3Provider>();
builder.Services.AddScoped<GeminiProvider>();

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

builder.Services.AddScoped<ConventionChatService>();
builder.Services.AddScoped<ConventionIndexingService>();
builder.Services.AddScoped<IScheduleUploadService, ScheduleUploadService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IAuthService, AuthService>();

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

builder.Services.AddHealthChecks()
    .AddCheck<LlmProviderHealthCheck>("llm_provider")
    .AddCheck<VectorStoreHealthCheck>("vector_store")
    .AddCheck<EmbeddingServiceHealthCheck>("embedding_service")
    .AddDbContextCheck<ConventionDbContext>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowSPA");
app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");
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
