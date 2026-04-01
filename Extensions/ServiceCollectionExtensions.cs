using LocalRAG.Configuration;
using LocalRAG.Interfaces;
using LocalRAG.Services.Auth;
using LocalRAG.Services.Convention;
using LocalRAG.Services.Flight;
using LocalRAG.Services.Shared;
using LocalRAG.Services.Upload;
using LocalRAG.Services.UserProfile;
using LocalRAG.Services.Admin;

namespace LocalRAG.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 인증 서비스 등록
    /// </summary>
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IVerificationService, VerificationService>();

        return services;
    }

    /// <summary>
    /// 컨벤션 도메인 서비스 등록
    /// </summary>
    public static IServiceCollection AddConventionServices(this IServiceCollection services)
    {
        services.AddScoped<INoticeService, NoticeService>();
        services.AddScoped<INoticeCategoryService, NoticeCategoryService>();
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IActionOrchestrationService, ActionOrchestrationService>();
        services.AddScoped<IUserActionService, UserActionService>();
        services.AddScoped<IChecklistService, ChecklistService>();
        services.AddScoped<IFormBuilderService, LocalRAG.Services.FormBuilder.FormBuilderService>();
        services.AddScoped<IPersonalTripService, LocalRAG.Services.PersonalTrip.PersonalTripService>();
        services.AddScoped<IConventionCrudService, ConventionCrudService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ITravelAssignmentService, LocalRAG.Services.Convention.TravelAssignmentService>();

        return services;
    }

    /// <summary>
    /// SMS 서비스 등록
    /// </summary>
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddScoped<ISmsSender, DbSmsSender>();
        services.AddScoped<ISmsService, LocalRAG.Services.Shared.SmsService>();
        services.AddSingleton<ITemplateVariableService, TemplateVariableService>();
        services.AddScoped<SmsTemplateContextFactory>();
        services.AddScoped<IKakaoAlimtalkService, KakaoAlimtalkService>();

        return services;
    }

    /// <summary>
    /// 파일 업로드 서비스 등록
    /// </summary>
    public static IServiceCollection AddUploadServices(this IServiceCollection services)
    {
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IUserUploadService, UserUploadService>();
        services.AddScoped<IScheduleTemplateUploadService, ScheduleUploadService>();
        services.AddScoped<IAttributeUploadService, AttributeUploadService>();
        services.AddScoped<IGroupScheduleMappingService, GroupScheduleMappingService>();
        services.AddScoped<INameTagUploadService, NameTagUploadService>();
        services.AddScoped<IOptionTourUploadService, OptionTourUploadService>();

        return services;
    }

    /// <summary>
    /// 사용자 컨텍스트 및 기타 서비스 등록
    /// </summary>
    public static IServiceCollection AddUserContextServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHttpClient<IFlightService, FlightService>();

        return services;
    }

    /// <summary>
    /// 사용자 프로필 서비스 등록
    /// </summary>
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserProfileService, UserProfileService>();

        return services;
    }

    /// <summary>
    /// 관리자 서비스 등록
    /// </summary>
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminStatsService, AdminStatsService>();
        services.AddScoped<ICompanionService, CompanionService>();

        return services;
    }

    /// <summary>
    /// 모든 도메인 서비스를 한 번에 등록
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthServices();
        services.AddConventionServices();
        services.AddUserServices();
        services.AddAdminServices();
        services.AddSmsServices();
        services.AddUploadServices();
        services.AddUserContextServices();

        return services;
    }
}
