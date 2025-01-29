using System.Reflection;
using System.Text.Json;

namespace CustomEventSourcing.Lib;

public static class StreamExtensions
{
    public static T ConstructFromStream<T>(this IStream stream) where T : class
    {
        if (stream.Events.Count == 0) throw new InvalidOperationException("No events");

        var firstEvent = stream.Events[0];
        var firstEventType = Type.GetType(firstEvent.DotnetType)!;
        var firstEventObj = JsonSerializer.Deserialize(firstEvent.Data, firstEventType)!;

        var ctor = typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.Instance, [firstEventType])!;
        var obj = ctor.Invoke([firstEventObj]) as T;

        foreach (var evnt in stream.Events.Skip(1))
        {
            var evntType = Type.GetType(evnt.DotnetType)!;
            var evntObj = JsonSerializer.Deserialize(evnt.Data, evntType)!;
            var apply = typeof(T).GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance,
                [evntType]);
            apply!.Invoke(obj, [evntObj]);
        }

        return obj!;
    }
}