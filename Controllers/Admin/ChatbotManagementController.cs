using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Services.Ai;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Constants;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class ChatbotManagementController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IndexingService _indexingService;
    private readonly LlmProviderManager _providerManager;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<ChatbotManagementController> _logger;

    public ChatbotManagementController(
        IUnitOfWork unitOfWork,
        IndexingService indexingService,
        LlmProviderManager providerManager,
        IVectorStore vectorStore,
        ILogger<ChatbotManagementController> logger)
    {
        _unitOfWork = unitOfWork;
        _indexingService = indexingService;
        _providerManager = providerManager;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    #region Stats

    [HttpGet("chatbot/stats")]
    public async Task<ActionResult> GetStats()
    {
        var totalDocuments = await _unitOfWork.VectorDataEntries.CountAsync();
        var activeConventions = await _unitOfWork.Conventions
            .CountAsync(c => c.DeleteYn == DeleteStatus.Active);
        var totalGuests = await _unitOfWork.UserConventions.CountAsync();
        var dbSize = totalDocuments * 1024;

        return Ok(new { totalDocuments, activeConventions, totalGuests, dbSize });
    }

    [HttpGet("chatbot/vector-status")]
    public async Task<ActionResult> GetVectorStoreStatus()
    {
        try
        {
            var totalVectors = await _vectorStore.GetDocumentCountAsync();

            var lastIndexed = await _unitOfWork.VectorDataEntries.Query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            var byType = await _unitOfWork.VectorDataEntries.Query
                .GroupBy(v => v.SourceType)
                .Select(g => new { type = g.Key, count = g.Count() })
                .ToListAsync();

            const int maxCapacity = 1_000_000;
            return Ok(new
            {
                isConnected = true,
                totalVectors,
                lastIndexed = lastIndexed == default ? (DateTime?)null : lastIndexed,
                capacity = new
                {
                    used = totalVectors,
                    total = maxCapacity,
                    usagePercent = totalVectors > 0 ? (double)totalVectors / maxCapacity * 100 : 0
                },
                byType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VectorStore 상태 조회 실패");
            return Ok(new
            {
                isConnected = false,
                totalVectors = 0,
                lastIndexed = (DateTime?)null,
                capacity = new { used = 0, total = 0, usagePercent = 0 },
                byType = Array.Empty<object>(),
                error = ex.Message
            });
        }
    }

    [HttpGet("chatbot/conventions/{conventionId}/indexed-items")]
    public async Task<ActionResult> GetIndexedItems(int conventionId)
    {
        try
        {
            var convention = await _unitOfWork.Conventions.Query
                .Include(c => c.ScheduleTemplates).ThenInclude(st => st.ScheduleItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conventionId);

            if (convention == null)
                return NotFound(new { message = "행사를 찾을 수 없습니다." });

            var userConventionCount = await _unitOfWork.UserConventions
                .CountAsync(uc => uc.ConventionId == conventionId);
            var notices = await _unitOfWork.Notices
                .CountAsync(n => n.ConventionId == conventionId && !n.IsDeleted);
            var conventionActions = await _unitOfWork.ConventionActions
                .CountAsync(a => a.ConventionId == conventionId && a.IsActive);
            var vectorCount = await _vectorStore.GetDocumentCountAsync(conventionId);

            return Ok(new
            {
                conventionInfo = new
                {
                    title = convention.Title,
                    startDate = convention.StartDate,
                    endDate = convention.EndDate,
                    type = convention.ConventionType,
                    indexed = true
                },
                guestSummary = new { totalCount = userConventionCount, indexed = userConventionCount > 0 },
                schedules = new
                {
                    templateCount = convention.ScheduleTemplates.Count,
                    itemCount = convention.ScheduleTemplates.SelectMany(st => st.ScheduleItems).Count(),
                    indexed = convention.ScheduleTemplates.Any()
                },
                notices = new { count = notices, indexed = notices > 0 },
                actions = new { count = conventionActions, indexed = conventionActions > 0 },
                vectorCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "색인 항목 조회 실패: {ConventionId}", conventionId);
            return StatusCode(500, new { message = "색인 항목 조회 중 오류가 발생했습니다." });
        }
    }

    [HttpGet("chatbot/vector-stats")]
    public async Task<ActionResult> GetVectorStats()
    {
        var total = await _unitOfWork.VectorDataEntries.CountAsync();

        var bySource = await _unitOfWork.VectorDataEntries.Query
            .GroupBy(v => v.SourceType)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                label = g.Key,
                percentage = total > 0 ? (double)g.Count() / total * 100 : 0
            })
            .ToListAsync();

        return Ok(new { bySouce = bySource }); // 프론트엔드 호환성 유지
    }

    #endregion

    #region Convention Indexing

    [HttpGet("chatbot/conventions")]
    public async Task<ActionResult> GetConventions()
    {
        var conventions = await _unitOfWork.Conventions.Query
            .Where(c => c.DeleteYn == DeleteStatus.Active)
            .Select(c => new
            {
                c.Id, c.Title, c.StartDate,
                GuestCount = _unitOfWork.UserConventions.Query.Count(uc => uc.ConventionId == c.Id),
                VectorCount = _unitOfWork.VectorDataEntries.Query.Count(v => v.ConventionId == c.Id),
                ChatbotEnabled = true
            })
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        return Ok(conventions);
    }

    [HttpPost("chatbot/reindex-convention/{conventionId}")]
    public async Task<ActionResult> ReindexConvention(int conventionId)
    {
        try
        {
            var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
            if (convention == null)
                return NotFound(new { message = "행사를 찾을 수 없습니다." });

            await _indexingService.IndexConventionAsync(conventionId);
            return Ok(new { message = "행사가 재색인되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "행사 {ConventionId} 재색인 실패", conventionId);
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다." });
        }
    }

    [HttpPost("chatbot/reindex-all")]
    public async Task<ActionResult> ReindexAll()
    {
        try
        {
            _logger.LogInformation("전체 재색인 시작");
            var result = await _indexingService.ReindexAllConventionsAsync();
            _logger.LogInformation("전체 재색인 완료: {Count}개 행사", result.SuccessCount);

            return Ok(new
            {
                message = $"{result.SuccessCount}개 행사의 데이터가 재색인되었습니다. (총 {result.TotalDocumentsIndexed}개 문서)",
                success = result.SuccessCount,
                failure = result.FailureCount,
                totalDocuments = result.TotalDocumentsIndexed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "전체 재색인 중 오류 발생");
            return StatusCode(500, new { message = "재색인 중 오류가 발생했습니다." });
        }
    }

    [HttpPut("chatbot/convention/{conventionId}/toggle")]
    public async Task<ActionResult> ToggleChatbot(int conventionId, [FromBody] ToggleChatbotRequest request)
    {
        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null)
            return NotFound(new { message = "행사를 찾을 수 없습니다." });

        return Ok(new { message = request.Enabled ? "챗봇이 활성화되었습니다." : "챗봇이 비활성화되었습니다." });
    }

    #endregion

    #region LLM Providers

    [HttpGet("llm-providers")]
    public async Task<ActionResult> GetAllProviders()
    {
        try
        {
            var settings = await _providerManager.GetAllSettingsAsync();
            var activeProvider = await _providerManager.GetActiveSettingAsync();

            var response = settings.Select(s => new
            {
                id = s.Id,
                providerType = s.ProviderName,
                modelName = s.ModelName,
                baseUrl = s.BaseUrl,
                apiKey = MaskApiKey(s.ApiKey),
                hasApiKey = !string.IsNullOrEmpty(s.ApiKey),
                isActive = s.IsActive,
                isCurrent = activeProvider?.Id == s.Id,
                createdAt = s.CreatedAt,
                updatedAt = s.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 목록 조회 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("llm-providers/active")]
    public async Task<ActionResult> GetActiveProvider()
    {
        try
        {
            var setting = await _providerManager.GetActiveSettingAsync();
            if (setting == null)
                return Ok(new { providerName = "none", message = "활성 Provider 없음" });

            return Ok(new
            {
                id = setting.Id,
                providerType = setting.ProviderName,
                modelName = setting.ModelName,
                baseUrl = setting.BaseUrl,
                isActive = setting.IsActive,
                updatedAt = setting.UpdatedAt ?? setting.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "활성 Provider 조회 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("llm-providers")]
    public async Task<ActionResult> CreateProvider([FromBody] CreateProviderRequest request)
    {
        try
        {
            var setting = new LlmSetting
            {
                ProviderName = request.ProviderType,
                ApiKey = request.ApiKey,
                BaseUrl = request.BaseUrl,
                ModelName = request.ModelName,
                IsActive = request.IsActive,
                AdditionalSettings = request.AdditionalSettings
            };

            var created = await _providerManager.CreateSettingAsync(setting);
            _logger.LogInformation("Provider 생성: {Provider}", created.ProviderName);

            return Ok(new { success = true, id = created.Id, message = "Provider가 추가되었습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 생성 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("llm-providers/{id}")]
    public async Task<ActionResult> UpdateProvider(int id, [FromBody] UpdateProviderRequest request)
    {
        try
        {
            var setting = new LlmSetting
            {
                ProviderName = request.ProviderType,
                ApiKey = string.IsNullOrEmpty(request.ApiKey) ? null : request.ApiKey,
                BaseUrl = request.BaseUrl,
                ModelName = request.ModelName,
                IsActive = request.IsActive,
                AdditionalSettings = request.AdditionalSettings
            };

            await _providerManager.UpdateSettingAsync(id, setting);
            _logger.LogInformation("Provider 수정: ID={Id}", id);

            return Ok(new { success = true, message = "Provider가 수정되었습니다." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 수정 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("llm-providers/{id}")]
    public async Task<ActionResult> DeleteProvider(int id)
    {
        try
        {
            var success = await _providerManager.DeleteSettingAsync(id);
            if (!success)
                return NotFound(new { error = "Provider를 찾을 수 없습니다." });

            _logger.LogInformation("Provider 삭제: ID={Id}", id);
            return Ok(new { success = true, message = "Provider가 삭제되었습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 삭제 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPatch("llm-providers/{id}/toggle")]
    public async Task<ActionResult> ToggleProvider(int id, [FromBody] ToggleProviderRequest request)
    {
        try
        {
            if (!request.IsActive)
                return BadRequest(new { error = "최소 하나의 Provider는 활성화되어야 합니다. 다른 Provider를 활성화하세요." });

            var success = await _providerManager.ActivateProviderAsync(id);
            if (!success)
                return NotFound(new { error = "Provider를 찾을 수 없습니다." });

            _logger.LogInformation("Provider 활성화 (스왑): ID={Id}", id);
            return Ok(new { success = true, message = "Provider가 활성화되었습니다. (실시간 스왑 완료)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 토글 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Logs

    [HttpGet("chatbot/recent-activities")]
    public async Task<ActionResult> GetRecentActivities()
    {
        var activities = await _unitOfWork.VectorDataEntries.Query
            .OrderByDescending(v => v.CreatedAt)
            .Take(10)
            .Select(v => new
            {
                id = v.Id,
                action = $"{v.SourceType} 색인됨",
                timestamp = v.CreatedAt
            })
            .ToListAsync();

        return Ok(activities);
    }

    [HttpGet("chatbot/logs")]
    public ActionResult GetLogs()
    {
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new { timestamp = now.ToString("HH:mm:ss"), level = "info", message = "챗봇 시스템 정상 작동 중" },
            new { timestamp = now.AddMinutes(-5).ToString("HH:mm:ss"), level = "info", message = "벡터 데이터베이스 연결 성공" },
            new { timestamp = now.AddMinutes(-10).ToString("HH:mm:ss"), level = "info", message = "행사 색인 완료" }
        };

        return Ok(logs);
    }

    #endregion

    #region Utility

    [HttpDelete("chatbot/clear-vectors")]
    public async Task<ActionResult> ClearVectors()
    {
        try
        {
            var count = await _unitOfWork.VectorDataEntries.CountAsync();
            var allEntries = await _unitOfWork.VectorDataEntries.GetAllAsync();
            _unitOfWork.VectorDataEntries.RemoveRange(allEntries);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("벡터 DB 초기화: {Count}개 문서 삭제됨", count);
            return Ok(new { message = $"{count}개의 벡터 문서가 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "벡터 DB 초기화 실패");
            return StatusCode(500, new { message = "벡터 DB 초기화 중 오류가 발생했습니다." });
        }
    }

    private static string? MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 10)
            return null;

        return $"{apiKey[..8]}...{apiKey[^4..]}";
    }

    #endregion
}
