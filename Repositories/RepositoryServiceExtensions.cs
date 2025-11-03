using LocalRAG.Data;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

/// <summary>
/// Repository 서비스 등록을 위한 확장 메서드
/// 
/// 의존성 주입(DI) 설정 방법:
/// 1. Program.cs에서 builder.Services.AddRepositories() 호출
/// 2. 모든 Repository와 UnitOfWork가 DI 컨테이너에 등록됨
/// 3. 컨트롤러나 서비스에서 생성자 주입으로 사용
/// </summary>
public static class RepositoryServiceExtensions
{
    /// <summary>
    /// Repository 및 Unit of Work를 DI 컨테이너에 등록합니다.
    /// 
    /// 서비스 생명주기(Lifetime) 설명:
    /// - Scoped: HTTP 요청당 하나의 인스턴스 생성
    ///   → DbContext와 동일한 생명주기로 맞춤
    ///   → 같은 요청 내에서는 동일한 인스턴스 공유
    ///   → 요청 종료 시 자동으로 Dispose
    /// 
    /// Scoped를 사용하는 이유:
    /// 1. DbContext가 Scoped이므로 동일한 생명주기 필요
    /// 2. 요청 내 트랜잭션 일관성 보장
    /// 3. 메모리 효율성 (요청 종료 시 자동 정리)
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Unit of Work 등록
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 개별 Repository 등록 (필요시 직접 주입받을 수 있도록)
        services.AddScoped<IConventionRepository, ConventionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IGuestAttributeRepository, GuestAttributeRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IVectorStoreRepository, VectorStoreRepository>();

        return services;
    }

    /// <summary>
    /// DbContext와 Repository를 함께 등록하는 편의 메서드
    /// 
    /// 사용 예시 (Program.cs):
    /// builder.Services.AddConventionDataAccess(
    ///     builder.Configuration.GetConnectionString("DefaultConnection")
    /// );
    /// </summary>
    public static IServiceCollection AddConventionDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        // DbContext 등록
        services.AddDbContext<ConventionDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            
            // 개발 환경에서만 민감한 데이터 로깅 활성화
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Repository 등록
        services.AddRepositories();

        return services;
    }
}
