namespace CustomEventSourcing.Procurement;

public class Order
{
    public Guid Id { get; }
    public string CustomerName { get; private set; }
    public string Status { get; private set; } = "In Progress";

    private readonly List<OrderLine> _lines = [];

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public Order(OrderCreated orderCreated)
    {
        Id = orderCreated.OrderId;
        CustomerName = orderCreated.CustomerName;
    }

    public void Apply(ItemAdded itemAdded)
    {
        var existingLine = _lines.FirstOrDefault(line => line.ItemNumber == itemAdded.ItemNumber);
        if (existingLine == null)
        {
            _lines.Add(new OrderLine
            {
                ItemNumber = itemAdded.ItemNumber,
                Price = itemAdded.Price,
                Quantity = itemAdded.Quantity
            });
            return;
        }

        existingLine.Quantity += itemAdded.Quantity;
    }

    public void Apply(OrderSubmitted orderSubmitted)
    {
        Status = "Submitted";
    }
}

public class OrderLine
{
    public required string ItemNumber { get; init; }
    public required decimal Price { get; init; }
    public required int Quantity { get; set; }
}