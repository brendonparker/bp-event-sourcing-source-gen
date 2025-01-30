namespace CustomEventSourcing.Lib;

public interface IStream
{
    public Guid Id { get; }
    IReadOnlyList<Event> Events { get; }
    void AddEvent<T>(T @event) where T : class;
    int Version { get; }
}