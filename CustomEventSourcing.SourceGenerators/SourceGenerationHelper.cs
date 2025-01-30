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
        """"
        using System.Collections.Concurrent;
        
        namespace CustomEventSourcing.Lib;
        
        public delegate object EventSourceFactoryMethod(IReadOnlyList<Event> events);
        
        public static partial class EventSourceFactory
        {
            private static ConcurrentDictionary<Type, EventSourceFactoryMethod> _factories = new();
            
            public static void Register<T>(EventSourceFactoryMethod factoryMethod) =>
                _factories[typeof(T)] = factoryMethod;
                
            private static void RegisterIfNotRegistered<T>(EventSourceFactoryMethod factoryMethod) =>
                _factories.GetOrAdd(typeof(T), factoryMethod);
            
            public static T Build<T>(IReadOnlyList<Event> events) where T : class
            {
                Init();
                if (!_factories.ContainsKey(typeof(T)))
                {
                    throw new InvalidOperationException(
                    $"""
                    No factory method has been registered for {typeof(T)}.
                    
                    Please include an assembly attribute if you want to generate the code for this:
                    [assembly: CustomEventSourcing.AllowConstructionFromEvents(typeof({typeof(T).FullName}))]
                    
                    Otherwise, register an EventSourceFactoryMethod using EventSourceFactory.Register
                    """);
                }
                return _factories[typeof(T)](events) as T;
            }
        }
        """";
}