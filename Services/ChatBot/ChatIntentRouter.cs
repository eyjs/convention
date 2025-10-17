// ----------------- ChatIntentRouter.cs (수정된 최종 버전) -----------------

using LocalRAG.Interfaces;
using LocalRAG.Models;

namespace LocalRAG.Services.ChatBot; // 네임스페이스는 기존 프로젝트에 맞게 확인해주세요.

/// <summary>
/// 질문 의도 분류 및 System Instruction 관리
/// </summary>
public class ChatIntentRouter
{
    private readonly ILlmProvider _llmProvider;
    private readonly ILogger<ChatIntentRouter> _logger;

    public enum Intent
    {
        PersonalInfo,     // 내 정보
        PersonalSchedule, // 내 일정
        Event,            // 행사 정보
        General,          // 일반 질문
        Unknown
    }

    public ChatIntentRouter(ILlmProvider llmProvider, ILogger<ChatIntentRouter> logger)
    {
        _llmProvider = llmProvider;
        _logger = logger;
    }

    /// <summary>
    /// LLM을 통해 질문 의도를 분류하고, 결과를 안전하게 Enum으로 변환합니다.
    /// </summary>
    public async Task<Intent> GetIntentAsync(string question, List<ChatRequestMessage>? history)
    {
        try
        {
            // 1. Provider 호출은 그대로 유지 (이제 Provider는 안정적으로 동작합니다)
            string intentString = await _llmProvider.ClassifyIntentAsync(question, history);
            _logger.LogInformation("Raw intent string from provider: {IntentString}", intentString);

            // 2. 수동 파싱 대신 Enum.TryParse 사용 (가장 중요한 변경점)
            // Enum.TryParse는 대소문자를 무시하고(ignoreCase: true) 문자열을 Enum으로 변환해줍니다.
            if (Enum.TryParse<Intent>(intentString, true, out var parsedIntent))
            {
                _logger.LogInformation("Successfully parsed intent: {Intent}", parsedIntent);
                return parsedIntent;
            }

            // 3. 만약 Enum에 없는 새로운 값이 오더라도 안전하게 Unknown으로 처리
            _logger.LogWarning("Could not parse intent string '{IntentString}' to a known Intent. Defaulting to Unknown.", intentString);
            return Intent.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while classifying intent. Defaulting to Unknown.");
            return Intent.Unknown;
        }
    }

    /// <summary>
    /// 각 의도에 최적화된 시스템 프롬프트를 반환합니다.
    /// </summary>
    public string? GetSystemInstruction(Intent intent)
    {
        switch (intent)
        {
            case Intent.PersonalInfo:
            case Intent.PersonalSchedule:
                // 개인 정보나 스케줄을 물을 땐, 절대 요약하지 말라고 강력하게 지시
                return "You are a personal information assistant. Your absolute top priority is to deliver the information from the user's context **fully and accurately, without any summarization or omission.** " +
                   "You must respond in Korean. Format the output clearly based on the provided context structure.";

            case Intent.Event:
            case Intent.Unknown:
                // RAG 검색 시에는 요약을 허용하되, 컨텍스트에만 기반하라고 지시
                return @"You are a professional event guide. You MUST respond in Korean. Base your answers strictly on the provided 'Context'. " +
                  "If the context does not contain the answer, you MUST respond with '정보가 부족하여 답변할 수 없습니다.'";

            case Intent.General:
                // 일반 대화에 대한 지침
                return "You are a friendly conversational AI assistant for Star Tour. Respond to general conversation kindly and wittily in Korean.";

            default:
                // 예외 상황에 대한 기본 지침
                return "You are a helpful AI assistant. Please respond in Korean.";
        }
    }
}