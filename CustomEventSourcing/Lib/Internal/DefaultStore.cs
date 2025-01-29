using Microsoft.EntityFrameworkCore;

namespace CustomEventSourcing.Lib.Internal;

internal class DefaultStore(EventSourceDbContext dbContext) : IStore
{
    public IStream StartStream(Guid id) =>
        new Stream(dbContext, id, []);

    public async Task<IStream> LoadStreamAsync(Guid id)
    {
        var events = await dbContext.Events
            .Where(x => x.StreamId == id)
            .OrderBy(x => x.Version)
            .ToListAsync();

        return new Stream(dbContext, id, events);
    }

    public Task SaveChangesAsync() =>
        dbContext.SaveChangesAsync();
}