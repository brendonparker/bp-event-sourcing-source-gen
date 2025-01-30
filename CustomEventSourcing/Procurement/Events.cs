namespace CustomEventSourcing.Procurement;

public sealed record OrderCreated(
    Guid OrderId,
    string CustomerName);

public sealed record ItemAdded(
    string ItemNumber,
    decimal Price,
    int Quantity = 1);

public sealed record OrderSubmitted;