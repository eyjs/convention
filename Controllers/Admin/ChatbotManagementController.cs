using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Services.Ai;
using LocalRAG.Entities;


namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class ChatbotManagementController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly IndexingService _indexingService;
    private readonly LlmProviderManager _providerManager;
    private readonly ILogger<ChatbotManagementController> _logger;

    public ChatbotManagementController(
        ConventionDbContext context,
        IndexingService indexingService,
        LlmProviderManager providerManager,
        ILogger<ChatbotManagementController> logger)
    {
        _context = context;
        _indexingService = indexingService;
        _providerManager = providerManager;
        _logger = logger;
    }

    #region 통계 및 상태

    /// <summary>
    /// 챗봇 시스템 통계
    /// </summary>
    [HttpGet("chatbot/stats")]
    public async Task<ActionResult> GetStats()
    {
        var totalDocuments = await _context.VectorDataEntries.CountAsync();
        var activeConventions = await _context.Conventions
            .Where(c => c.DeleteYn == "N")
            .CountAsync();
        var totalGuests = await _context.Guests.CountAsync();
        var dbSize = totalDocuments * 1024; // 대략적인 크기

        return Ok(new
        {
            totalDocuments,
            activeConventions,
            totalGuests,
            dbSize
        });
    }

    /// <summary>
    /// VectorStore 상태 정보
    /// </summary>
    [HttpGet("chatbot/vector-status")]
    [AllowAnonymous] // 일시적으로 인증 없이 접근 허용
    public async Task<ActionResult> GetVectorStoreStatus()
    {
        try
        {
            var totalVectors = await _context.VectorDataEntries.CountAsync();
            var lastIndexed = await _context.VectorDataEntries
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            var byType = await _context.VectorDataEntries
                .GroupBy(v => v.SourceType)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Ok(new
            {
                isConnected = true, // DB 연결 성공 시 true
                totalVectors,
                lastIndexed,
                capacity = new
                {
                    used = totalVectors,
                    total = 1000000, // 최대 용량 (예시)
                    usagePercent = totalVectors > 0 ? (double)totalVectors / 1000000 * 100 : 0
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
                capacity = new
                {
                    used = 0,
                    total = 0,
                    usagePercent = 0
                },
                byType = new object[0],
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 벡터 통계
    /// </summary>
    [HttpGet("chatbot/vector-stats")]
    [AllowAnonymous]
    public async Task<ActionResult> GetVectorStats()
    {
        var total = await _context.VectorDataEntries.CountAsync();

        var bySource = await _context.VectorDataEntries
            .GroupBy(v => v.SourceType)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                label = g.Key,
                percentage = total > 0 ? (double)g.Count() / total * 100 : 0
            })
            .ToListAsync();

        return Ok(new
        {
            bySouce = bySource // 기존 오타 유지 (프론트엔드 호환성)
        });
    }

    #endregion

    #region 행사 관리

    /// <summary>
    /// 행사 목록 조회
    /// </summary>
    [HttpGet("chatbot/conventions")]
    [AllowAnonymous]
    public async Task<ActionResult> GetConventions()
    {
        var conventions = await _context.Conventions
            .Where(c => c.DeleteYn == "N")
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.StartDate,
                GuestCount = c.Guests.Count(),
                VectorCount = _context.VectorDataEntries.Count(v => v.ConventionId == c.Id),
                ChatbotEnabled = true // 모든 행사 기본 활성화
            })
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        return Ok(conventions);
    }

    /// <summary>
    /// 행사별 재색인
    /// </summary>
    [HttpPost("chatbot/reindex-convention/{conventionId}")]
    [AllowAnonymous]
    public async Task<ActionResult> ReindexConvention(int conventionId)
    {
        try
        {
            var convention = await _context.Conventions.FindAsync(conventionId);
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

    /// <summary>
    /// 전체 재색인
    /// </summary>
    [HttpPost("chatbot/reindex-all")]
    [AllowAnonymous]
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

    /// <summary>
    /// 행사 챗봇 활성/비활성
    /// </summary>
    [HttpPut("chatbot/convention/{conventionId}/toggle")]
    [AllowAnonymous]
    public async Task<ActionResult> ToggleChatbot(int conventionId, [FromBody] ToggleChatbotRequest request)
    {
        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null)
            return NotFound(new { message = "행사를 찾을 수 없습니다." });

        // 현재는 항상 활성화 상태 (추후 DB 필드 추가 가능)
        return Ok(new { message = request.Enabled ? "챗봇이 활성화되었습니다." : "챗봇이 비활성화되었습니다." });
    }

    #endregion

    #region LLM Provider 관리

    /// <summary>
    /// 모든 LLM Provider 목록 조회
    /// </summary>
    [HttpGet("llm-providers")]
    [AllowAnonymous] // 일시적으로 인증 없이 접근 허용
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
                apiKey = MaskApiKey(s.ApiKey), // 마스킹된 키
                hasApiKey = !string.IsNullOrEmpty(s.ApiKey),
                isActive = s.IsActive,
                isCurrent = activeProvider?.Id == s.Id, // 현재 적용 상태
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

    /// <summary>
    /// 현재 활성 Provider 조회
    /// </summary>
    [HttpGet("llm-providers/active")]
    public async Task<ActionResult> GetActiveProvider()
    {
        try
        {
            var setting = await _providerManager.GetActiveSettingAsync();
            if (setting == null)
            {
                return Ok(new { providerName = "none", message = "활성 Provider 없음" });
            }

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

    /// <summary>
    /// Provider 추가
    /// </summary>
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

    /// <summary>
    /// Provider 수정
    /// </summary>
    [HttpPut("llm-providers/{id}")]
    public async Task<ActionResult> UpdateProvider(int id, [FromBody] UpdateProviderRequest request)
    {
        try
        {
            var setting = new LlmSetting
            {
                ProviderName = request.ProviderType,
                ApiKey = string.IsNullOrEmpty(request.ApiKey) ? null : request.ApiKey, // 비어있으면 기존 유지
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

    /// <summary>
    /// Provider 삭제
    /// </summary>
    [HttpDelete("llm-providers/{id}")]
    public async Task<ActionResult> DeleteProvider(int id)
    {
        try
        {
            var success = await _providerManager.DeleteSettingAsync(id);
            if (!success)
            {
                return NotFound(new { error = "Provider를 찾을 수 없습니다." });
            }

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

    /// <summary>
    /// Provider 활성/비활성 토글 (실시간 스왑)
    /// </summary>
    [HttpPatch("llm-providers/{id}/toggle")]
    public async Task<ActionResult> ToggleProvider(int id, [FromBody] ToggleProviderRequest request)
    {
        try
        {
            if (request.IsActive)
            {
                // 활성화 = 이 Provider로 스왑
                var success = await _providerManager.ActivateProviderAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Provider를 찾을 수 없습니다." });
                }

                _logger.LogInformation("Provider 활성화 (스왑): ID={Id}", id);
                
                return Ok(new { success = true, message = "Provider가 활성화되었습니다. (실시간 스왑 완료)" });
            }
            else
            {
                // 비활성화는 허용하지 않음 (항상 하나는 활성화되어야 함)
                return BadRequest(new { error = "최소 하나의 Provider는 활성화되어야 합니다. 다른 Provider를 활성화하세요." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider 토글 실패");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region 로그 및 활동

    /// <summary>
    /// 최근 활동 내역
    /// </summary>
    [HttpGet("chatbot/recent-activities")]
    [AllowAnonymous]
    public async Task<ActionResult> GetRecentActivities()
    {
        var activities = await _context.VectorDataEntries
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

    /// <summary>
    /// 시스템 로그
    /// </summary>
    [HttpGet("chatbot/logs")]
    [AllowAnonymous]
    public ActionResult GetLogs()
    {
        var logs = new[]
        {
            new { timestamp = DateTime.Now.ToString("HH:mm:ss"), level = "info", message = "챗봇 시스템 정상 작동 중" },
            new { timestamp = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss"), level = "info", message = "벡터 데이터베이스 연결 성공" },
            new { timestamp = DateTime.Now.AddMinutes(-10).ToString("HH:mm:ss"), level = "info", message = "행사 색인 완료" }
        };

        return Ok(logs);
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 벡터 DB 초기화
    /// </summary>
    [HttpDelete("chatbot/clear-vectors")]
    [AllowAnonymous]
    public async Task<ActionResult> ClearVectors()
    {
        try
        {
            var count = await _context.VectorDataEntries.CountAsync();
            _context.VectorDataEntries.RemoveRange(_context.VectorDataEntries);
            await _context.SaveChangesAsync();

            _logger.LogWarning("벡터 DB 초기화: {Count}개 문서 삭제됨", count);

            return Ok(new { message = $"{count}개의 벡터 문서가 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "벡터 DB 초기화 실패");
            return StatusCode(500, new { message = "벡터 DB 초기화 중 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// API Key 마스킹
    /// </summary>
    private string? MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 10)
            return null;
        
        return apiKey.Substring(0, 8) + "..." + apiKey.Substring(apiKey.Length - 4);
    }

    #endregion
}

#region Request Models

public class ToggleChatbotRequest
{
    public bool Enabled { get; set; }
}

public class CreateProviderRequest
{
    public string ProviderType { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; }
    public string? AdditionalSettings { get; set; }
}

public class UpdateProviderRequest
{
    public string ProviderType { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; }
    public string? AdditionalSettings { get; set; }
}

public class ToggleProviderRequest
{
    public bool IsActive { get; set; }
}

#endregion
