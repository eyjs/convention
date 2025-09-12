using LocalRAG.Interfaces;
using LocalRAG.Providers;
using LocalRAG.Services;
using LocalRAG.Storage;
using LocalRAG.Data;
using LocalRAG.Middleware;
using LocalRAG.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "LocalRAG Travel Management API", 
        Version = "v1",
        Description = "A comprehensive travel management system with RAG capabilities"
    });
});

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// HttpClient for LLM providers
builder.Services.AddHttpClient();

// Entity Framework
builder.Services.AddDbContext<TravelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Convention Database (읽기 전용)
builder.Services.AddDbContext<ConventionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnectionString")));

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

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<LlmProviderHealthCheck>("llm_provider", HealthStatus.Degraded, new[] { "llm" })
    .AddCheck<VectorStoreHealthCheck>("vector_store", HealthStatus.Degraded, new[] { "storage" })
    .AddCheck<EmbeddingServiceHealthCheck>("embedding_service", HealthStatus.Degraded, new[] { "embedding" })
    .AddDbContextCheck<TravelDbContext>("database", HealthStatus.Degraded, new[] { "database" });

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocalRAG Travel Management API v1");
        c.RoutePrefix = "swagger";
    });
}

// Custom middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Standard middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthorization();

// Health checks endpoint
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Tags = entry.Value.Tags
            })
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Map controllers
app.MapControllers();

// Default route
app.MapGet("/", () => new
{
    Application = "LocalRAG Travel Management System",
    Version = "1.0.0",
    Status = "Running",
    Endpoints = new
    {
        Api = "/api",
        Health = "/health",
        Swagger = "/swagger",
        SystemInfo = "/api/system/info"
    }
});

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
    }
}

Console.WriteLine("🚀 LocalRAG Travel Management System starting...");
Console.WriteLine($"📍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Swagger UI: {(app.Environment.IsDevelopment() ? "https://localhost:5001/swagger" : "Disabled in production")}");
Console.WriteLine($"💓 Health Check: https://localhost:5001/health");

app.Run();
