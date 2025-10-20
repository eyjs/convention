using Microsoft.AspNetCore.Mvc;

namespace LocalRAG.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/chatbot")]
    public class ChatbotAdminController : ControllerBase
    {
        private readonly ILogger<ChatbotAdminController> _logger;

        public ChatbotAdminController(ILogger<ChatbotAdminController> logger)
        {
            _logger = logger;
        }

        // 시스템 상태 확인
        [HttpGet("status")]
        public ActionResult GetSystemStatus()
        {
            // TODO: 실제 Ollama 연결 확인 로직 구현
            return Ok(new
            {
                overall = true,
                ollama = true,
                model = true,
                apiKey = true,
                message = "모든 시스템이 정상입니다"
            });
        }

        // 사용 가능한 모델 목록
        [HttpGet("models")]
        public ActionResult GetAvailableModels()
        {
            // TODO: 실제 Ollama API 호출하여 모델 목록 가져오기
            return Ok(new
            {
                currentModel = "llama2",
                models = new[]
                {
                    new { name = "llama2", size = 3826793677 },
                    new { name = "mistral", size = 4109865159 },
                    new { name = "codellama", size = 3825819519 }
                }
            });
        }

        // 모델 선택
        [HttpPost("select-model")]
        public ActionResult SelectModel([FromBody] SelectModelRequest request)
        {
            // TODO: 실제 모델 전환 로직 구현
            _logger.LogInformation($"Model changed to: {request.ModelName}");
            return Ok(new { success = true, model = request.ModelName });
        }

        // API 키 목록 조회
        [HttpGet("api-keys")]
        public ActionResult GetApiKeys()
        {
            // TODO: 실제 DB에서 API 키 목록 조회
            return Ok(new[]
            {
                new
                {
                    id = 1,
                    provider = "OpenAI",
                    keyValue = "sk-proj-xxxxxxxxxxxxxxxx",
                    isActive = true,
                    createdAt = DateTime.UtcNow.AddDays(-30)
                },
                new
                {
                    id = 2,
                    provider = "Anthropic",
                    keyValue = "sk-ant-xxxxxxxxxxxxxxxx",
                    isActive = false,
                    createdAt = DateTime.UtcNow.AddDays(-15)
                }
            });
        }

        // API 키 추가
        [HttpPost("api-keys")]
        public ActionResult CreateApiKey([FromBody] ApiKeyRequest request)
        {
            // TODO: 실제 DB에 API 키 저장
            _logger.LogInformation($"API Key created for: {request.Provider}");
            return Ok(new { success = true, id = 3 });
        }

        // API 키 수정
        [HttpPut("api-keys/{id}")]
        public ActionResult UpdateApiKey(int id, [FromBody] ApiKeyRequest request)
        {
            // TODO: 실제 DB에서 API 키 수정
            _logger.LogInformation($"API Key {id} updated");
            return Ok(new { success = true });
        }

        // API 키 삭제
        [HttpDelete("api-keys/{id}")]
        public ActionResult DeleteApiKey(int id)
        {
            // TODO: 실제 DB에서 API 키 삭제
            _logger.LogInformation($"API Key {id} deleted");
            return Ok(new { success = true });
        }

        // API 키 활성/비활성 토글
        [HttpPatch("api-keys/{id}/toggle")]
        public ActionResult ToggleApiKey(int id, [FromBody] ToggleApiKeyRequest request)
        {
            // TODO: 실제 DB에서 API 키 상태 변경
            _logger.LogInformation($"API Key {id} toggled to: {request.IsActive}");
            return Ok(new { success = true, isActive = request.IsActive });
        }
    }

    public class SelectModelRequest
    {
        public string ModelName { get; set; } = string.Empty;
    }

    public class ApiKeyRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ToggleApiKeyRequest
    {
        public bool IsActive { get; set; }
    }
}
