using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using System.Diagnostics;

namespace LocalRAG.Controllers.System;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmProvider _llmProvider;
    private readonly ILogger<SystemController> _logger;

    public SystemController(
        IVectorStore vectorStore,
        IEmbeddingService embeddingService,
        ILlmProvider llmProvider,
        ILogger<SystemController> logger)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _llmProvider = llmProvider;
        _logger = logger;
    }

    [HttpGet("info")]
    public async Task<ActionResult<object>> GetSystemInfo()
    {
        try
        {
            var documentCount = await _vectorStore.GetDocumentCountAsync();
            
            var systemInfo = new
            {
                Application = new
                {
                    Name = "LocalRAG Travel Management System",
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    StartTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                },
                LlmProvider = new
                {
                    Name = _llmProvider.ProviderName,
                    Status = "Active"
                },
                EmbeddingService = new
                {
                    Type = _embeddingService.GetType().Name,
                    Dimensions = _embeddingService.EmbeddingDimensions,
                    Status = "Active"
                },
                VectorStore = new
                {
                    Type = _vectorStore.GetType().Name,
                    DocumentCount = documentCount,
                    Status = "Active"
                },
                Runtime = new
                {
                    DotNetVersion = Environment.Version.ToString(),
                    Environment.ProcessorCount,
                    WorkingSet = $"{Environment.WorkingSet / 1024 / 1024} MB",
                    Environment.MachineName
                }
            };

            return Ok(systemInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system information");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<object>> GetMetrics()
    {
        try
        {
            var documentCount = await _vectorStore.GetDocumentCountAsync();
            var gcMemory = GC.GetTotalMemory(false);
            
            var metrics = new
            {
                System = new
                {
                    Timestamp = DateTime.UtcNow,
                    UptimeSeconds = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime).TotalSeconds,
                    CpuUsagePercent = await GetCpuUsageAsync(),
                    MemoryUsageMB = Environment.WorkingSet / 1024 / 1024,
                    GcMemoryMB = gcMemory / 1024 / 1024
                },
                VectorStore = new
                {
                    DocumentCount = documentCount,
                    EstimatedSizeMB = documentCount * _embeddingService.EmbeddingDimensions * 4 / 1024 / 1024 // float = 4 bytes
                },
                Performance = new
                {
                    ThreadPoolWorkerThreads = ThreadPool.ThreadCount,
                    ThreadPoolCompletionPortThreads = ThreadPool.CompletedWorkItemCount
                }
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system metrics");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("benchmark")]
    public async Task<ActionResult<object>> RunBenchmark()
    {
        try
        {
            var benchmark = new
            {
                StartTime = DateTime.UtcNow,
                Tests = new List<object>()
            };

            var tests = benchmark.Tests;

            // 임베딩 성능 테스트
            var embeddingStart = DateTime.UtcNow;
            await _embeddingService.GenerateEmbeddingAsync("This is a test sentence for benchmarking embedding generation performance.");
            var embeddingTime = (DateTime.UtcNow - embeddingStart).TotalMilliseconds;
            
            tests.Add(new
            {
                Test = "Embedding Generation",
                DurationMs = embeddingTime,
                Status = "Completed"
            });

            // LLM 성능 테스트
            var llmStart = DateTime.UtcNow;
            await _llmProvider.GenerateResponseAsync("Hello, this is a test prompt.");
            var llmTime = (DateTime.UtcNow - llmStart).TotalMilliseconds;
            
            tests.Add(new
            {
                Test = "LLM Response Generation",
                DurationMs = llmTime,
                Status = "Completed"
            });

            // 벡터 검색 성능 테스트
            var searchStart = DateTime.UtcNow;
            var testEmbedding = await _embeddingService.GenerateEmbeddingAsync("test query");
            await _vectorStore.SearchAsync(testEmbedding, 5);
            var searchTime = (DateTime.UtcNow - searchStart).TotalMilliseconds;
            
            tests.Add(new
            {
                Test = "Vector Search",
                DurationMs = searchTime,
                Status = "Completed"
            });

            var totalTime = (DateTime.UtcNow - benchmark.StartTime).TotalMilliseconds;

            return Ok(new
            {
                benchmark.StartTime,
                EndTime = DateTime.UtcNow,
                TotalDurationMs = totalTime,
                Tests = tests,
                Summary = new
                {
                    FastestTest = tests.OrderBy(t => ((dynamic)t).DurationMs).First(),
                    SlowestTest = tests.OrderByDescending(t => ((dynamic)t).DurationMs).First(),
                    AverageTimeMs = tests.Average(t => ((dynamic)t).DurationMs)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run benchmark");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    private static async Task<double> GetCpuUsageAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            
            await Task.Delay(1000); // 1초 대기
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return cpuUsageTotal * 100;
        }
        catch
        {
            return 0; // CPU 사용률을 가져올 수 없는 경우
        }
    }
}
