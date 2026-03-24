using LocalRAG.Data;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using LocalRAG.Entities.Flight;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.Entities.PersonalTrip;
using LocalRAG.DTOs.ScheduleModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace LocalRAG.Repositories;

/// <summary>
/// Unit of Work 구현체
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ConventionDbContext _context;
    private IDbContextTransaction? _transaction;

    // Specialized Repository 인스턴스들 (Lazy Initialization)
    private IConventionRepository? _conventions;
    private IUserRepository? _users;
    private IUserConventionRepository? _userConventions;
    private ICommentRepository? _comments;
    private ISurveyRepository? _surveys;
    private IGuestAttributeRepository? _guestAttributes;
    private IFeatureRepository? _features;
    private IMenuRepository? _menus;
    private ISectionRepository? _sections;
    private IOwnerRepository? _owners;
    private IVectorStoreRepository? _vectorStores;

    // Generic Repository 인스턴스들
    private IRepository<ConventionAction>? _conventionActions;
    private IRepository<UserActionStatus>? _userActionStatuses;
    private IRepository<ScheduleTemplate>? _scheduleTemplates;
    private IRepository<OptionTour>? _optionTours;
    private IRepository<UserOptionTour>? _userOptionTours;
    private IRepository<ScheduleItem>? _scheduleItems;
    private IRepository<GuestScheduleTemplate>? _guestScheduleTemplates;
    private IRepository<Notice>? _notices;
    private IRepository<NoticeCategory>? _noticeCategories;
    private IRepository<Gallery>? _galleries;
    private IRepository<GalleryImage>? _galleryImages;
    private IRepository<FileAttachment>? _fileAttachments;
    private IRepository<AttributeDefinition>? _attributeDefinitions;
    private IRepository<AttributeTemplate>? _attributeTemplates;
    private IRepository<ActionTemplate>? _actionTemplates;
    private IRepository<ActionSubmission>? _actionSubmissions;
    private IRepository<ConventionChatMessage>? _conventionChatMessages;
    private IRepository<LlmSetting>? _llmSettings;
    private IRepository<VectorDataEntry>? _vectorDataEntries;
    private IRepository<SurveyQuestion>? _surveyQuestions;
    private IRepository<QuestionOption>? _questionOptions;
    private IRepository<SurveyResponse>? _surveyResponses;
    private IRepository<SurveyResponseDetail>? _surveyResponseDetails;
    private IRepository<SmsLog>? _smsLogs;
    private IRepository<SmsTemplate>? _smsTemplates;
    private IRepository<FormDefinition>? _formDefinitions;
    private IRepository<FormField>? _formFields;
    private IRepository<FormSubmission>? _formSubmissions;
    private IRepository<Entities.PersonalTrip.PersonalTrip>? _personalTrips;
    private IRepository<Flight>? _flights;
    private IRepository<Accommodation>? _accommodations;
    private IRepository<ItineraryItem>? _itineraryItems;
    private IRepository<ChecklistCategory>? _checklistCategories;
    private IRepository<ChecklistItem>? _checklistItems;
    private IRepository<CompanionRelation>? _companionRelations;
    private IRepository<ScheduleImage>? _scheduleImages;
    private IRepository<IncheonFlightData>? _incheonFlightDatas;

    public UnitOfWork(ConventionDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ============================================================
    // Specialized Repository Properties
    // ============================================================

    public IConventionRepository Conventions =>
        _conventions ??= new ConventionRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IUserConventionRepository UserConventions =>
        _userConventions ??= new UserConventionRepository(_context);

    public ICommentRepository Comments =>
        _comments ??= new CommentRepository(_context);

    public ISurveyRepository Surveys =>
        _surveys ??= new SurveyRepository(_context);

    public IGuestAttributeRepository GuestAttributes =>
        _guestAttributes ??= new GuestAttributeRepository(_context);

    public IFeatureRepository Features =>
        _features ??= new FeatureRepository(_context);

    public IMenuRepository Menus =>
        _menus ??= new MenuRepository(_context);

    public ISectionRepository Sections =>
        _sections ??= new SectionRepository(_context);

    public IOwnerRepository Owners =>
        _owners ??= new OwnerRepository(_context);

    public IVectorStoreRepository VectorStores =>
        _vectorStores ??= new VectorStoreRepository(_context);

    // ============================================================
    // Generic Repository Properties
    // ============================================================

    public IRepository<ConventionAction> ConventionActions =>
        _conventionActions ??= new Repository<ConventionAction>(_context);

    public IRepository<UserActionStatus> UserActionStatuses =>
        _userActionStatuses ??= new Repository<UserActionStatus>(_context);

    public IRepository<ScheduleTemplate> ScheduleTemplates =>
        _scheduleTemplates ??= new Repository<ScheduleTemplate>(_context);

    public IRepository<OptionTour> OptionTours =>
        _optionTours ??= new Repository<OptionTour>(_context);

    public IRepository<UserOptionTour> UserOptionTours =>
        _userOptionTours ??= new Repository<UserOptionTour>(_context);

    public IRepository<ScheduleItem> ScheduleItems =>
        _scheduleItems ??= new Repository<ScheduleItem>(_context);

    public IRepository<GuestScheduleTemplate> GuestScheduleTemplates =>
        _guestScheduleTemplates ??= new Repository<GuestScheduleTemplate>(_context);

    public IRepository<Notice> Notices =>
        _notices ??= new Repository<Notice>(_context);

    public IRepository<NoticeCategory> NoticeCategories =>
        _noticeCategories ??= new Repository<NoticeCategory>(_context);

    public IRepository<Gallery> Galleries =>
        _galleries ??= new Repository<Gallery>(_context);

    public IRepository<GalleryImage> GalleryImages =>
        _galleryImages ??= new Repository<GalleryImage>(_context);

    public IRepository<FileAttachment> FileAttachments =>
        _fileAttachments ??= new Repository<FileAttachment>(_context);

    public IRepository<AttributeDefinition> AttributeDefinitions =>
        _attributeDefinitions ??= new Repository<AttributeDefinition>(_context);

    public IRepository<AttributeTemplate> AttributeTemplates =>
        _attributeTemplates ??= new Repository<AttributeTemplate>(_context);

    public IRepository<ActionTemplate> ActionTemplates =>
        _actionTemplates ??= new Repository<ActionTemplate>(_context);

    public IRepository<ActionSubmission> ActionSubmissions =>
        _actionSubmissions ??= new Repository<ActionSubmission>(_context);

    public IRepository<ConventionChatMessage> ConventionChatMessages =>
        _conventionChatMessages ??= new Repository<ConventionChatMessage>(_context);

    public IRepository<LlmSetting> LlmSettings =>
        _llmSettings ??= new Repository<LlmSetting>(_context);

    public IRepository<VectorDataEntry> VectorDataEntries =>
        _vectorDataEntries ??= new Repository<VectorDataEntry>(_context);

    public IRepository<SurveyQuestion> SurveyQuestions =>
        _surveyQuestions ??= new Repository<SurveyQuestion>(_context);

    public IRepository<QuestionOption> QuestionOptions =>
        _questionOptions ??= new Repository<QuestionOption>(_context);

    public IRepository<SurveyResponse> SurveyResponses =>
        _surveyResponses ??= new Repository<SurveyResponse>(_context);

    public IRepository<SurveyResponseDetail> SurveyResponseDetails =>
        _surveyResponseDetails ??= new Repository<SurveyResponseDetail>(_context);

    public IRepository<SmsLog> SmsLogs =>
        _smsLogs ??= new Repository<SmsLog>(_context);

    public IRepository<SmsTemplate> SmsTemplates =>
        _smsTemplates ??= new Repository<SmsTemplate>(_context);

    public IRepository<FormDefinition> FormDefinitions =>
        _formDefinitions ??= new Repository<FormDefinition>(_context);

    public IRepository<FormField> FormFields =>
        _formFields ??= new Repository<FormField>(_context);

    public IRepository<FormSubmission> FormSubmissions =>
        _formSubmissions ??= new Repository<FormSubmission>(_context);

    // Flight (Incheon Airport)
    public IRepository<IncheonFlightData> IncheonFlightDatas =>
        _incheonFlightDatas ??= new Repository<IncheonFlightData>(_context);

    // Companion
    public IRepository<CompanionRelation> CompanionRelations =>
        _companionRelations ??= new Repository<CompanionRelation>(_context);

    // Schedule Images
    public IRepository<ScheduleImage> ScheduleImages =>
        _scheduleImages ??= new Repository<ScheduleImage>(_context);

    // Personal Trip
    public IRepository<Entities.PersonalTrip.PersonalTrip> PersonalTrips =>
        _personalTrips ??= new Repository<Entities.PersonalTrip.PersonalTrip>(_context);

    public IRepository<Flight> Flights =>
        _flights ??= new Repository<Flight>(_context);

    public IRepository<Accommodation> Accommodations =>
        _accommodations ??= new Repository<Accommodation>(_context);

    public IRepository<ItineraryItem> ItineraryItems =>
        _itineraryItems ??= new Repository<ItineraryItem>(_context);

    public IRepository<ChecklistCategory> ChecklistCategories =>
        _checklistCategories ??= new Repository<ChecklistCategory>(_context);

    public IRepository<ChecklistItem> ChecklistItems =>
        _checklistItems ??= new Repository<ChecklistItem>(_context);

    // ============================================================
    // Transaction Methods
    // ============================================================

    /// <summary>
    /// 모든 변경사항을 데이터베이스에 저장합니다.
    /// 
    /// 작동 원리:
    /// 1. EF Core의 Change Tracker가 추적한 모든 변경사항 확인
    /// 2. 변경사항들을 SQL 명령으로 변환
    /// 3. 데이터베이스에 한 번에 전송 (단일 트랜잭션)
    /// 4. 성공: 변경된 행의 수 반환
    /// 5. 실패: 예외 발생 및 자동 롤백
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // 예외 발생 시 Change Tracker의 변경사항은 유지됨
            // 필요시 재시도하거나 수동으로 롤백 가능
            throw;
        }
    }

    /// <summary>
    /// 명시적인 트랜잭션을 시작합니다.
    /// 
    /// 사용 시나리오:
    /// - 복잡한 비즈니스 로직에서 여러 SaveChangesAsync 호출이 필요한 경우
    /// - 일부 작업 후 조건에 따라 커밋/롤백 결정이 필요한 경우
    /// 
    /// 예시:
    /// await BeginTransactionAsync();
    /// try
    /// {
    ///     await Conventions.AddAsync(convention);
    ///     await SaveChangesAsync();
    ///     
    ///     // 추가 로직...
    ///     
    ///     await CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     await RollbackTransactionAsync();
    ///     throw;
    /// }
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// 트랜잭션을 커밋합니다.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to commit.");
        }

        try
        {
            // 먼저 변경사항 저장
            await _context.SaveChangesAsync(cancellationToken);
            // 그 다음 트랜잭션 커밋
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // 커밋 실패 시 자동 롤백
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// 트랜잭션을 롤백합니다.
    /// 
    /// 롤백 효과:
    /// - 데이터베이스: 트랜잭션 시작 이후의 모든 변경사항 취소
    /// - Change Tracker: 추적 중인 엔티티 상태는 유지됨 (주의!)
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // ============================================================
    // Dispose Pattern (리소스 정리)
    // ============================================================
    
    /// <summary>
    /// 리소스를 정리합니다.
    /// 
    /// 정리 대상:
    /// - DbContext: 데이터베이스 연결 해제
    /// - Transaction: 진행 중인 트랜잭션 롤백
    /// 
    /// using 문과 함께 사용하면 자동으로 호출됩니다.
    /// </summary>
    public void Dispose()
    {
        // 트랜잭션이 진행 중이면 롤백
        _transaction?.Dispose();
        
        // DbContext 정리
        _context?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
