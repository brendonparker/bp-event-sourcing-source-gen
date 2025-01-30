using CustomEventSourcing;
using CustomEventSourcing.Lib;
using CustomEventSourcing.Lib.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddEventSourcing(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EventSourceDbContext>(opts =>
        {
            opts.UseSqlite(connectionString)
                .UseSnakeCaseNamingConvention();
        });
        services.AddTransient<IDatabaseSetup, DefaultDatabaseSetup>();
        services.TryAddTransient<IStore, DefaultStore>();
    }
}