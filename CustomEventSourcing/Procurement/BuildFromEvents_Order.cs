using System.Text.Json;

namespace CustomEventSourcing.Procurement;

/// <summary>
/// Purely for demo-purposes.
/// We don't need this shanks to the source-generator
/// </summary>
public class BuildFromEvents_Order
{
    public static Order Build(IReadOnlyList<Event> events)
    {
        Order? order = null;

        foreach (var evnt in events)
        {
            if (evnt.DotnetType == typeof(OrderCreated).FullName!)
            {
                var eventObj = JsonSerializer.Deserialize<OrderCreated>(evnt.Data)!;
                order = new Order(eventObj);
                continue;
            }

            if (evnt.DotnetType == typeof(ItemAdded).FullName!)
            {
                var eventObj = JsonSerializer.Deserialize<ItemAdded>(evnt.Data)!;
                order?.Apply(eventObj);
                continue;
            }

            if (evnt.DotnetType == typeof(OrderSubmitted).FullName!)
            {
                var eventObj = JsonSerializer.Deserialize<OrderSubmitted>(evnt.Data)!;
                order?.Apply(eventObj);
                continue;
            }

            throw new NotImplementedException();
        }

        return order!;
    }
}