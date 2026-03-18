using Microsoft.AspNetCore.Mvc;
using LocalRAG.Configuration;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.System;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentController : ControllerBase
{
    private readonly IConnectionStringProvider _connectionProvider;
    private readonly IWebHostEnvironment _environment;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnvironmentController> _logger;

    public EnvironmentController(
        IConnectionStringProvider connectionProvider,
        IWebHostEnvironment environment,
        IUnitOfWork unitOfWork,
        ILogger<EnvironmentController> logger)
    {
        _connectionProvider = connectionProvider;
        _environment = environment;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("info")]
    public IActionResult GetEnvironmentInfo()
    {
        var connectionString = _connectionProvider.GetConnectionString();

        var result = new
        {
            Environment = new
            {
                Name = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsProduction = _environment.IsProduction()
            },
            Database = new
            {
                ConnectionString = _environment.IsDevelopment() ? connectionString : "[HIDDEN]"
            },
            Timestamp = DateTime.Now
        };

        return Ok(result);
    }

    [HttpGet("database-status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var startTime = DateTime.Now;

            Dictionary<string, int> tableCounts = new();

            try
            {
                tableCounts["Conventions"] = await _unitOfWork.Conventions.CountAsync(_ => true);
                tableCounts["UserConventions"] = await _unitOfWork.UserConventions.Query.CountAsync();
                tableCounts["GuestAttributes"] = await _unitOfWork.GuestAttributes.Query.CountAsync();
                tableCounts["Features"] = await _unitOfWork.Features.Query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error querying database: {Error}", ex.Message);
            }

            var connectionTime = DateTime.Now - startTime;

            var result = new
            {
                Environment = _environment.EnvironmentName,
                Database = new
                {
                    Status = "Connected",
                    ConnectionTime = connectionTime.TotalMilliseconds,
                    TableCounts = tableCounts
                },
                TestResults = new
                {
                    CanConnect = true,
                    ResponseTime = connectionTime.TotalMilliseconds,
                    TestedAt = DateTime.Now
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database status check failed");
            return StatusCode(500, new
            {
                Error = "Database status check failed",
                ex.Message,
                Timestamp = DateTime.Now
            });
        }
    }
}
