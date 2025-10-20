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
            var canConnect = await _context.Database.CanConnectAsync();
            var connectionTime = DateTime.Now - startTime;
            
            Dictionary<string, int> tableCounts = new();
            
            if (canConnect)
            {
                try
                {
                    tableCounts["Conventions"] = await _context.Conventions.CountAsync();
                    tableCounts["Guests"] = await _context.Guests.CountAsync();
                    tableCounts["Schedules"] = await _context.Schedules.CountAsync();
                    tableCounts["GuestAttributes"] = await _context.GuestAttributes.CountAsync();
                    tableCounts["Features"] = await _context.Features.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error querying database: {Error}", ex.Message);
                }
            }

            var result = new
            {
                Environment = _environment.EnvironmentName,
                Database = new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    ConnectionTime = connectionTime.TotalMilliseconds,
                    TableCounts = tableCounts
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
            _logger.LogError(ex, "Database status check failed");
            return StatusCode(500, new
            {
                Error = "Database status check failed",
                Message = ex.Message,
                Timestamp = DateTime.Now
            });
        }
    }
}
