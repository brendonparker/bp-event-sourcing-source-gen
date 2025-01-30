using Microsoft.Extensions.DependencyInjection;

namespace CustomEventSourcing.Lib.Internal;

internal class DefaultDatabaseSetup(EventSourceDbContext dbContext) : IDatabaseSetup
{
    public void EnsureDeleted() => dbContext.Database.EnsureDeleted();
    public void EnsureCreated() => dbContext.Database.EnsureCreated();
}