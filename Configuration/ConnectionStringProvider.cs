namespace LocalRAG.Configuration;

public interface IConnectionStringProvider
{
    string GetConnectionString();
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public ConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection이 설정되지 않았습니다.");
    }
}

public static class ConnectionStringExtensions
{
    public static IServiceCollection AddConnectionStringProvider(this IServiceCollection services)
    {
        // Singleton으로 변경: 연결 문자열은 요청별로 변경되지 않으며, DbContextFactory와 호환성 보장
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        return services;
    }
}
