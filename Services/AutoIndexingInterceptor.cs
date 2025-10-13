using LocalRAG.Data;
using LocalRAG.Interfaces;
using LocalRAG.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalRAG.Services; // Added this using statement

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
        var indexingService = scope.ServiceProvider.GetService<AdvancedConventionIndexingService>();

        if (indexingService == null)
        {
            _logger.LogWarning("Indexing services not available");
            return;
        }

        var changedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var changedConventionIds = new HashSet<int>();

        foreach (var entry in changedEntries)
        {
            int? conventionId = null;
            switch (entry.Entity)
            {
                case Convention convention:
                    conventionId = convention.Id;
                    break;
                case Guest guest:
                    conventionId = guest.ConventionId; // Assuming Guest has ConventionId
                    break;
                case ScheduleTemplate template:
                    conventionId = template.ConventionId; // Assuming ScheduleTemplate has ConventionId
                    break;
                case ScheduleItem item:
                    // Need to find the ConventionId from the ScheduleTemplate that owns this ScheduleItem
                    // This might require loading the parent ScheduleTemplate or having ConventionId directly on ScheduleItem
                    // For now, assuming ScheduleItem has ScheduleTemplateId
                    if (item.ScheduleTemplateId > 0) // Check if it's a valid ID
                    {
                        var scheduleTemplate = await context.Set<ScheduleTemplate>().AsNoTracking().FirstOrDefaultAsync(st => st.Id == item.ScheduleTemplateId);
                        conventionId = scheduleTemplate?.ConventionId;
                    }
                    break;
            }

            if (conventionId.HasValue)
            {
                changedConventionIds.Add(conventionId.Value);
            }
        }

        foreach (var id in changedConventionIds)
        {
            try
            {
                _logger.LogInformation("Auto-indexing Convention {ConventionId} due to entity change.", id);
                await indexingService.IndexConventionAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-index convention {ConventionId}", id);
            }
        }
    }
}
