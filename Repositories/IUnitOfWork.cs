using LocalRAG.Entities;
using LocalRAG.DTOs.ScheduleModels;

namespace LocalRAG.Repositories;

/// <summary>
/// Unit of Work 인터페이스
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // --- Repository Properties ---
    IConventionRepository Conventions { get; }
    IUserRepository Users { get; }
    IUserConventionRepository UserConventions { get; }
    ICommentRepository Comments { get; }
    ISurveyRepository Surveys { get; }
    IGuestAttributeRepository GuestAttributes { get; }
    IFeatureRepository Features { get; }
    IMenuRepository Menus { get; }
    ISectionRepository Sections { get; }
    IOwnerRepository Owners { get; }
    IVectorStoreRepository VectorStores { get; }
    IRepository<Entities.Action.ConventionAction> ConventionActions { get; }
    IRepository<UserActionStatus> UserActionStatuses { get; }
    IRepository<ScheduleTemplate> ScheduleTemplates { get; }
    IRepository<OptionTour> OptionTours { get; }
    IRepository<UserOptionTour> UserOptionTours { get; }

    // --- Transaction Methods ---
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

// ============================================================
// 각 엔티티별 Repository 인터페이스
// ============================================================

/// <summary>
/// UserConvention Repository 인터페이스
/// </summary>
public interface IUserConventionRepository : IRepository<UserConvention>
{
    Task<IEnumerable<UserConvention>> GetByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserConvention>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserConvention?> GetByUserAndConventionAsync(int userId, int conventionId, CancellationToken cancellationToken = default);
    Task<UserConvention?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserConvention>> GetByGroupNameAsync(int conventionId, string groupName, CancellationToken cancellationToken = default);
    Task<UserConvention?> GetUserConventionWithUserAsync(int conventionId, string userName, string userPhone, CancellationToken cancellationToken = default);
}

/// <summary>
/// GuestAttribute Repository 인터페이스
/// </summary>
public interface IGuestAttributeRepository : IRepository<GuestAttribute>
{
    Task<IEnumerable<GuestAttribute>> GetAttributesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<GuestAttribute?> GetAttributeByKeyAsync(int userId, string attributeKey, CancellationToken cancellationToken = default);
    Task UpsertAttributeAsync(int userId, string attributeKey, string attributeValue, CancellationToken cancellationToken = default);
}

/// <summary>
/// Feature Repository 인터페이스
/// </summary>
public interface IFeatureRepository : IRepository<Feature>
{
    Task<IEnumerable<Feature>> GetFeaturesByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Feature>> GetEnabledFeaturesAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<bool> IsFeatureEnabledAsync(int conventionId, string featureName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Menu Repository 인터페이스
/// </summary>
public interface IMenuRepository : IRepository<Menu>
{
    Task<IEnumerable<Menu>> GetMenusByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<Menu?> GetMenuWithSectionsAsync(int menuId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Section Repository 인터페이스
/// </summary>
public interface ISectionRepository : IRepository<Section>
{
    Task<IEnumerable<Section>> GetSectionsByMenuIdAsync(int menuId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Owner Repository 인터페이스
/// </summary>
public interface IOwnerRepository : IRepository<Owner>
{
    Task<IEnumerable<Owner>> GetOwnersByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// VectorStore Repository 인터페이스
/// </summary>
public interface IVectorStoreRepository : IRepository<VectorStore>
{
    Task<IEnumerable<VectorStore>> GetVectorsByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VectorStore>> GetVectorsBySourceAsync(string sourceTable, string sourceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Comment Repository 인터페이스
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByNoticeIdAsync(int noticeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Survey Repository 인터페이스
/// </summary>
public interface ISurveyRepository : IRepository<Survey>
{
    Task<Survey?> GetSurveyWithQuestionsAndOptionsAsync(int surveyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Survey>> GetSurveysByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default);
}
