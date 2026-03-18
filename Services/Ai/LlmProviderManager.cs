using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Providers;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Ai;

public class LlmProviderManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LlmProviderManager> _logger;
    private ILlmProvider? _cachedProvider;
    private string? _cachedProviderName;

    public LlmProviderManager(
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<LlmProviderManager> logger)
    {
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ILlmProvider> GetActiveProviderAsync()
    {
        var activeSetting = await _unitOfWork.LlmSettings.Query
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .FirstOrDefaultAsync();

        if (activeSetting == null)
        {
            _logger.LogWarning("No active LLM provider found, using default");
            return _serviceProvider.GetRequiredService<Llama3Provider>();
        }

        // 캐시된 Provider와 동일하면 재사용
        if (_cachedProvider != null && _cachedProviderName == activeSetting.ProviderName)
        {
            return _cachedProvider;
        }

        // Provider 생성 - DB 설정 기반
        var httpClient = _serviceProvider.GetRequiredService<HttpClient>();
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        _cachedProvider = activeSetting.ProviderName.ToLower() switch
        {
            "llama3" => new Llama3Provider(httpClient, configuration, loggerFactory.CreateLogger<Llama3Provider>()),
            "gemini" => CreateGeminiProvider(httpClient, activeSetting),
            "google" => CreateGeminiProvider(httpClient, activeSetting),
            _ => new Llama3Provider(httpClient, configuration, loggerFactory.CreateLogger<Llama3Provider>())
        };

        _cachedProviderName = activeSetting.ProviderName;
        _logger.LogInformation("LLM Provider switched to: {Provider} (Model: {Model}, BaseUrl: {BaseUrl})",
            _cachedProviderName, activeSetting.ModelName, activeSetting.BaseUrl);

        return _cachedProvider;
    }

    private ILlmProvider CreateGeminiProvider(HttpClient httpClient, LlmSetting setting)
    {
        return new GeminiProvider(httpClient, setting);
    }

    public async Task<LlmSetting?> GetActiveSettingAsync()
    {
        return await _unitOfWork.LlmSettings.Query
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LlmSetting>> GetAllSettingsAsync()
    {
        return await _unitOfWork.LlmSettings.Query
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.ProviderName)
            .ToListAsync();
    }

    public async Task<LlmSetting> CreateSettingAsync(LlmSetting setting)
    {
        var existing = await _unitOfWork.LlmSettings.Query
            .FirstOrDefaultAsync(s => s.ProviderName == setting.ProviderName);

        if (existing != null)
        {
            throw new InvalidOperationException($"Provider '{setting.ProviderName}' already exists");
        }

        await _unitOfWork.LlmSettings.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        _cachedProvider = null;
        _cachedProviderName = null;

        return setting;
    }

    public async Task<LlmSetting> UpdateSettingAsync(int id, LlmSetting setting)
    {
        var existing = await _unitOfWork.LlmSettings.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"LlmSetting with ID {id} not found");
        }

        existing.ProviderName = setting.ProviderName;
        existing.ApiKey = setting.ApiKey;
        existing.BaseUrl = setting.BaseUrl;
        existing.ModelName = setting.ModelName;
        existing.IsActive = setting.IsActive;
        existing.AdditionalSettings = setting.AdditionalSettings;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LlmSettings.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        _cachedProvider = null;
        _cachedProviderName = null;

        return existing;
    }

    public async Task<bool> ActivateProviderAsync(int id)
    {
        var setting = await _unitOfWork.LlmSettings.GetByIdAsync(id);
        if (setting == null) return false;

        // 모든 설정을 비활성화
        await _unitOfWork.LlmSettings.Query
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false));

        setting.IsActive = true;
        setting.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LlmSettings.Update(setting);
        await _unitOfWork.SaveChangesAsync();

        _cachedProvider = null;
        _cachedProviderName = null;

        _logger.LogInformation("Activated LLM Provider: {Provider}", setting.ProviderName);
        return true;
    }

    public async Task<bool> DeleteSettingAsync(int id)
    {
        var setting = await _unitOfWork.LlmSettings.GetByIdAsync(id);
        if (setting == null) return false;

        if (setting.IsActive)
        {
            throw new InvalidOperationException("Cannot delete active provider");
        }

        _unitOfWork.LlmSettings.Remove(setting);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
