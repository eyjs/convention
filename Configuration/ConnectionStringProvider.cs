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
        services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();
        return services;
    }
}
