using LocalRAG.Interfaces;
using LocalRAG.Models;

namespace LocalRAG.Services.Chat;

public class SourceIdentifier
{
    private readonly ILlmProvider _llmProvider;
    private readonly ILogger<SourceIdentifier> _logger;

    public SourceIdentifier(ILlmProvider llmProvider, ILogger<SourceIdentifier> logger)
    {
        _llmProvider = llmProvider;
        _logger = logger;
    }

    public enum SourceType
    {
        Personal_DB,
        Schedule_DB,
        RAG_Convention,
        RAG_Global,
        General_LLM
    }

    public async Task<List<SourceType>> GetRequiredSourcesAsync(
        string question, 
        List<ChatRequestMessage>? history = null)
    {
        try
        {
            var sourcesText = await CallLlmForSourceIdentificationAsync(question, history);
            var sources = ParseSourcesFromLlmResponse(sourcesText);
            
            _logger.LogInformation(
                "Identified {Count} sources for question: [{Sources}]",
                sources.Count,
                string.Join(", ", sources));
            
            return sources;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying sources, falling back to General_LLM");
            return new List<SourceType> { SourceType.General_LLM };
        }
    }

    private async Task<string> CallLlmForSourceIdentificationAsync(
        string question,
        List<ChatRequestMessage>? history)
    {
        var prompt = BuildSourceIdentificationPrompt(question);
        
        // ILlmProvider의 ClassifyIntentAsync를 활용하거나 직접 호출
        var response = await _llmProvider.GenerateResponseAsync(
            prompt: prompt,
            context: null,
            history: history,
            systemInstructionOverride: "You are a query routing assistant. Respond ONLY with comma-separated source names.");

        return response?.Trim() ?? "General_LLM";
    }

    private string BuildSourceIdentificationPrompt(string question)
    {
        return $@"Analyze this question and identify which data sources are needed.

Available Data Sources:
1. Personal_DB: User's personal information (name, contact, organization, dietary restrictions)
2. Schedule_DB: User's personal schedule and calendar (their sessions, meetings, appointments)
3. RAG_Convention: Information about the current convention (sessions, speakers, venues, policies)
4. RAG_Global: General knowledge and past convention information
5. General_LLM: General questions that don't require any specific data

Instructions:
- Return ONLY a comma-separated list of required source names
- You can select multiple sources if needed
- Be precise: only include truly necessary sources

Examples:
""What's on my schedule tomorrow at the main venue?"" → Schedule_DB, RAG_Convention
""Who is speaking at the AI session?"" → RAG_Convention
""What is my dietary restriction?"" → Personal_DB
""Hello, how are you?"" → General_LLM

Question: {question}

Response (comma-separated sources only):";
    }

    private List<SourceType> ParseSourcesFromLlmResponse(string llmResponse)
    {
        var sources = new List<SourceType>();

        if (string.IsNullOrWhiteSpace(llmResponse))
        {
            _logger.LogWarning("Empty LLM response for source identification");
            return new List<SourceType> { SourceType.General_LLM };
        }

        var sourceNames = llmResponse
            .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s));

        foreach (var sourceName in sourceNames)
        {
            if (Enum.TryParse<SourceType>(sourceName, ignoreCase: true, out var sourceType))
            {
                if (!sources.Contains(sourceType))
                {
                    sources.Add(sourceType);
                }
            }
            else
            {
                _logger.LogWarning("Invalid source name from LLM: '{SourceName}'. Skipping.", sourceName);
            }
        }

        if (sources.Count == 0)
        {
            _logger.LogWarning("No valid sources parsed from LLM response: '{Response}'. Using General_LLM.", llmResponse);
            sources.Add(SourceType.General_LLM);
        }

        return sources;
    }
}
