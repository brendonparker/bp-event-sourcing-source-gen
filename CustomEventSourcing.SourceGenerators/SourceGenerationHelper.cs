namespace CustomEventSourcing.SourceGenerators;

internal class SourceGenerationHelper
{
    public const string Attribute =
        """
        namespace CustomEventSourcing;

        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        public sealed class AllowConstructionFromEventsAttribute(Type type) : Attribute
        {
            public Type Type { get; } = type;
        }
        """;
    
    public const string Registrations =
        """
        namespace CustomEventSourcing.Generated;
        
        public static partial class EventSourceFactoryRegistrations;
        """;
    
    public const string EventSourceFactory =
        """
        using System.Collections.Concurrent;
        
        namespace CustomEventSourcing.Lib;
        
        public static partial class EventSourceFactory
        {
            private static ConcurrentDictionary<Type, Func<IReadOnlyList<Event>, object>> _factories = new();
            
            public static void Register<T>(Func<IReadOnlyList<Event>, object> factoryMethod) =>
                _factories[typeof(T)] = factoryMethod;
                
            private static void RegisterIfNotRegistered<T>(Func<IReadOnlyList<Event>, object> factoryMethod) =>
                _factories.GetOrAdd(typeof(T), factoryMethod);
            
            public static T Build<T>(IReadOnlyList<Event> events) where T : class
            {
                Init();
                return _factories[typeof(T)](events) as T;
            }
        }
        """;
}