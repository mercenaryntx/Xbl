using Microsoft.Extensions.DependencyInjection;

namespace Xbl.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddData(this IServiceCollection services, params string[] databases)
    {
        foreach (var database in databases)
        {
            services.AddKeyedScoped<IDatabaseContext>(database, (_, _) => new DatabaseContext(database));
        }
        return services;
    }
}