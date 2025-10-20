using Microsoft.AspNetCore.Mvc;
using LocalRAG.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using LocalRAG.Interfaces;
using System.Collections.Generic; // List<Notice> 사용

namespace LocalRAG.Controllers.Ai;

[ApiController]
[Route("api/admin/indexing")]
[Authorize(Roles = "Admin")]
public class IndexingController : ControllerBase
{
    private readonly IndexingService _conventionIndexingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<IndexingController> _logger;

    public IndexingController(
        IndexingService conventionIndexingService,
        IVectorStore vectorStore,
        ILogger<IndexingController> logger)
    {
        _conventionIndexingService = conventionIndexingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    /// <summary>
    /// 특정 컨벤션의 모든 데이터를 다시 색인합니다.
    /// </summary>
    [HttpPost("conventions/{conventionId}/reindex")]
    public async Task<IActionResult> ReindexConvention(int conventionId)
    {
        try
        {
            _logger.LogInformation("Starting re-indexing for Convention ID: {ConventionId}", conventionId);
            var indexedCount = await _conventionIndexingService.IndexConventionAsync(conventionId);
            _logger.LogInformation("Successfully re-indexed {Count} documents for Convention ID: {ConventionId}", indexedCount, conventionId);
            return Ok(new { message = $"{indexedCount}개의 문서를 성공적으로 색인했습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to re-index data for Convention ID: {ConventionId}", conventionId);
            return StatusCode(500, new { message = "데이터를 색인하는 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 특정 컨벤션의 현재 색인 상태를 조회합니다.
    /// </summary>
    [HttpGet("conventions/{conventionId}/status")]
    public async Task<IActionResult> GetStatus(int conventionId)
    {
        try
        {
            var documentCount = await _vectorStore.GetDocumentCountAsync(conventionId);
            return Ok(new { documentCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get indexing status for Convention ID: {ConventionId}", conventionId);
            return StatusCode(500, new { message = "색인 상태를 조회하는 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 모든 컨벤션의 데이터를 다시 색인합니다.
    /// </summary>
    [HttpPost("reindex")]
    public async Task<IActionResult> ReindexAll()
    {
        try
        {
            _logger.LogInformation("Starting global re-indexing.");
            var result = await _conventionIndexingService.ReindexAllConventionsAsync();
            _logger.LogInformation("Successfully re-indexed {Count} documents from all sources.", result.TotalDocumentsIndexed);
            return Ok(new { message = $"{result.TotalDocumentsIndexed}개의 문서를 성공적으로 색인했습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to re-index all data.");
            return StatusCode(500, new { message = "전체 데이터를 색인하는 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 전체 색인 상태를 조회합니다.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetGlobalStatus()
    {
        try
        {
            var documentCount = await _vectorStore.GetDocumentCountAsync();
            return Ok(new { documentCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get global indexing status.");
            return StatusCode(500, new { message = "전체 색인 상태를 조회하는 중 오류가 발생했습니다." });
        }
    }
}
