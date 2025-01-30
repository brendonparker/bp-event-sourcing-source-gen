namespace CustomEventSourcing.SourceGenerators;

public readonly record struct EventSourcedClassToGenerate
{
    public readonly string Type;

    public EventSourcedClassToGenerate(string type)
    {
        Type = type;
    }
}