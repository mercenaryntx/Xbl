using System.Diagnostics.CodeAnalysis;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.Extensions.DependencyInjection;

namespace Xbl.Data.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddData(this IServiceCollection services, params string[] databases)
    {
        MicroOrmConfig.SqlProvider = SqlProvider.SQLite;
        foreach (var database in databases)
        {
            services.AddKeyedScoped<IDatabaseContext>(database, (s, _) => new DatabaseContext(database, s.GetRequiredService<GlobalConfig>()));
        }
        return services;
    }
}