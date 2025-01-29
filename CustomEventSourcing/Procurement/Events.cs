namespace CustomEventSourcing.Procurement;

public sealed record OrderCreated(
    Guid OrderId,
    string CustomerName);

public sealed record ItemAdded(
    string ItemNumber,
    decimal Price,
    int Quantity);

public sealed record OrderSubmitted;