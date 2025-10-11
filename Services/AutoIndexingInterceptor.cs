using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LocalRAG.Services;

/// <summary>
/// 엔티티 변경 시 자동으로 색인을 수행하는 인터셉터
/// </summary>
public class AutoIndexingInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoIndexingInterceptor> _logger;
    private readonly IConfiguration _configuration;

    public AutoIndexingInterceptor(
        IServiceProvider serviceProvider, 
        ILogger<AutoIndexingInterceptor> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // 변경된 엔티티 수집
        var entitiesToIndex = CollectEntitiesToIndex(eventData.Context);

        // 저장 전 엔티티 정보 저장 (SaveChanges 후 ID가 생성되므로)
        eventData.Context.ChangeTracker.DetectChanges();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            // SaveChanges 완료 후 색인 수행
            await IndexChangedEntitiesAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<(object Entity, string State)> CollectEntitiesToIndex(DbContext context)
    {
        var entitiesToIndex = new List<(object Entity, string State)>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // 색인이 필요한 엔티티 타입만 필터링
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity is Convention || 
                    entry.Entity is Guest || 
                    entry.Entity is ScheduleTemplate ||
                    entry.Entity is ScheduleItem)
                {
                    entitiesToIndex.Add((entry.Entity, entry.State.ToString()));
                }
            }
        }

        return entitiesToIndex;
    }

    private async Task IndexChangedEntitiesAsync(DbContext context, CancellationToken cancellationToken)
    {
        // 서비스 스코프 생성 (인터셉터는 싱글톤이므로)
        using var scope = _serviceProvider.CreateScope();
        var indexingService = scope.ServiceProvider.GetService<ConventionIndexingService>();
        var ragService = scope.ServiceProvider.GetService<IRagService>();

        if (indexingService == null || ragService == null)
        {
            _logger.LogWarning("Indexing services not available");
            return;
        }

        var changedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in changedEntries)
        {
            try
            {
                switch (entry.Entity)
                {
                    case Convention convention:
                        await IndexConventionAsync(convention, indexingService, ragService);
                        break;

                    case Guest guest:
                        await IndexGuestAsync(guest, indexingService, ragService);
                        break;

                    case ScheduleTemplate template:
                        await IndexScheduleTemplateAsync(template, indexingService, ragService);
                        break;

                    case ScheduleItem item:
                        await IndexScheduleItemAsync(item, indexingService, ragService);
                        break;
                }
            }
            catch (Exception ex)
            {
                // 색인 실패는 로그만 남기고 계속 진행
                _logger.LogError(ex, "Failed to index entity: {EntityType}", entry.Entity.GetType().Name);
            }
        }
    }

    private async Task IndexConventionAsync(Convention convention, ConventionIndexingService indexingService, IRagService ragService)
    {
        try
        {
            _logger.LogInformation("Auto-indexing Convention: {Id} - {Title}", convention.Id, convention.Title);
            
            var text = $"행사명: {convention.Title}\n" +
                      $"유형: {convention.ConventionType}\n" +
                      $"시작일: {convention.StartDate:yyyy-MM-dd}\n" +
                      $"종료일: {convention.EndDate:yyyy-MM-dd}";

            var metadata = new Dictionary<string, object>
            {
                ["convention_id"] = convention.Id,
                ["type"] = "convention",
                ["title"] = convention.Title,
                ["convention_type"] = convention.ConventionType
            };

            await ragService.AddDocumentAsync(text, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index convention {Id}", convention.Id);
        }
    }

    private async Task IndexGuestAsync(Guest guest, ConventionIndexingService indexingService, IRagService ragService)
    {
        try
        {
            _logger.LogInformation("Auto-indexing Guest: {Id} - {Name}", guest.Id, guest.GuestName);
            
            var text = $"참석자명: {guest.GuestName}\n" +
                      $"전화번호: {guest.Telephone}\n" +
                      $"부서: {guest.CorpPart}\n" +
                      $"주민번호: {guest.ResidentNumber}\n" +
                      $"소속: {guest.Affiliation}";

            var metadata = new Dictionary<string, object>
            {
                ["convention_id"] = guest.ConventionId,
                ["guest_id"] = guest.Id,
                ["type"] = "guest",
                ["name"] = guest.GuestName
            };

            await ragService.AddDocumentAsync(text, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index guest {Id}", guest.Id);
        }
    }

    private async Task IndexScheduleTemplateAsync(ScheduleTemplate template, ConventionIndexingService indexingService, IRagService ragService)
    {
        try
        {
            _logger.LogInformation("Auto-indexing ScheduleTemplate: {Id} - {Name}", template.Id, template.CourseName);
            
            var text = $"일정 코스: {template.CourseName}\n" +
                      $"설명: {template.Description}";

            var metadata = new Dictionary<string, object>
            {
                ["convention_id"] = template.ConventionId,
                ["template_id"] = template.Id,
                ["type"] = "schedule_template",
                ["course_name"] = template.CourseName
            };

            await ragService.AddDocumentAsync(text, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index schedule template {Id}", template.Id);
        }
    }

    private async Task IndexScheduleItemAsync(ScheduleItem item, ConventionIndexingService indexingService, IRagService ragService)
    {
        try
        {
            _logger.LogInformation("Auto-indexing ScheduleItem: {Id} - {Title}", item.Id, item.Title);
            
            var text = $"일정: {item.Title}\n" +
                      $"날짜: {item.ScheduleDate:yyyy-MM-dd}\n" +
                      $"시작시간: {item.StartTime}\n" +
                      $"내용: {item.Content}";

            var metadata = new Dictionary<string, object>
            {
                ["schedule_item_id"] = item.Id,
                ["type"] = "schedule_item",
                ["title"] = item.Title,
                ["date"] = item.ScheduleDate.ToString("yyyy-MM-dd")
            };

            await ragService.AddDocumentAsync(text, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index schedule item {Id}", item.Id);
        }
    }
}
