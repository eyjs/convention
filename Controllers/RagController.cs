using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Models.DTOs;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagController> _logger;

    public RagController(IRagService ragService, ILogger<RagController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    [HttpPost("documents")]
    public async Task<ActionResult<AddDocumentResponse>> AddDocument([FromBody] AddDocumentRequest request)
    {
        try
        {
            var documentId = await _ragService.AddDocumentAsync(request.Content, request.Metadata);
            
            return Ok(new AddDocumentResponse(documentId, true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add document");
            
            return Ok(new AddDocumentResponse(string.Empty, false, ex.Message));
        }
    }

    [HttpPost("query")]
    public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
    {
        try
        {
            var result = await _ragService.QueryAsync(request.Question, request.TopK);
            
            var response = new QueryResponse(
                result.Answer,
                result.Sources.Select(s => new SourceDocument(
                    s.DocumentId,
                    s.Content,
                    s.Similarity,
                    s.Metadata ?? new Dictionary<string, object>()
                )).ToList(),
                result.LlmProvider
            );
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process query: {Question}", request.Question);
            
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var stats = await _ragService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("documents/{documentId}")]
    public async Task<ActionResult<object>> DeleteDocument(string documentId)
    {
        try
        {
            var result = await _ragService.DeleteDocumentAsync(documentId);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document: {DocumentId}", documentId);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
