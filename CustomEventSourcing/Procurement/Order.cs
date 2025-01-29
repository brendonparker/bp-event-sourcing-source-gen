namespace CustomEventSourcing.Procurement;

public class Order
{
}

public sealed record OrderCreated(
    Guid OrderId,
    string CustomerName);

public sealed record ItemAdded(
    string ItemNumber,
    decimal Price,
    int Quantity);

public sealed record OrderSubmitted;