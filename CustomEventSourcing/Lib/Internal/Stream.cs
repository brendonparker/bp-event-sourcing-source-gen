using System.Text.Json;

namespace CustomEventSourcing.Lib.Internal;

internal class Stream : IStream
{
    public Guid Id { get; }
    private readonly List<Event> _events;
    private readonly EventSourceDbContext _dbContext;
    public IReadOnlyList<Event> Events => _events.AsReadOnly();
    public int Version { get; private set; }

    public Stream(EventSourceDbContext dbContext, Guid streamId, List<Event> events)
    {
        _dbContext = dbContext;
        Id = streamId;
        _events = events;
        if (events.Count > 0)
        {
            Version = events.Max(x => x.Version);
        }
    }

    public void AddEvent<T>(T @event) where T : class =>
        _dbContext.Events.Add(new Event
        {
            Id = Guid.NewGuid(),
            StreamId = Id,
            Version = Version++,
            Data = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType()),
            DotnetType = @event.GetType().FullName!,
            Timestamp = DateTime.UtcNow
        });
}