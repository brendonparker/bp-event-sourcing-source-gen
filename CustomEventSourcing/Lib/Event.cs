namespace CustomEventSourcing;

public sealed class Event
{
    public required Guid Id { get; init; }
    public required Guid StreamId { get; init; }
    public required int Version { get; init; }
    public required byte[] Data { get; init; }
    public required string DotnetType { get; init; }
    public required DateTime Timestamp { get; init; }
}