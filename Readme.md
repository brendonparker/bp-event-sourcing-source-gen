# Lightweight Event Sourcing with Code Generation

This is a quick demo app/library to showcase the following:
 - Using event sourcing
 - Using Source Generators to write boilerplate code.
 - Use EF (with sqlite) as backing store.

## Background/Motivation

I've spent a little time looking at [Marten](https://martendb.io/). I liked the ideas and concepts, but was overwhelmed by the size and features of the library. I wanted to play with something a little more lightweight. I really liked how they did some code generation to remove some of the boilerplate. I wanted to play with these concepts/ideas myself.

In the past when I've played with this there was usually some AggregateBase base class that needed to do a lot as far as tracking events, and replaying events from the existing stream to rebuild state. Inevitably you either had to write some fancy/expensive dynamic code or use a bunch of reflection to make that happen.

I like keeping the aggregate/entity as lightweight as possible. Really just the logic for handling the events and managing state. It really should need to track new events or know how to route existing events to their handlers. So as much as possible, I'm trying to carve that responsibility out elsewhere, and use source generators to handle the boilerplate.

## How it works

1. **Create a POCO**

    It can have either a parameterless constructor, or a constructor that takes in an event.
    
    In my example, I opted for the latter with a constructor that take the _OrderCreated_ event.

    ```csharp
    public class Order
    {
        public Guid Id { get; }
        public string CustomerName { get; private set; }
        
        public Order(OrderCreated orderCreated)
        {
            Id = orderCreated.OrderId;
            CustomerName = orderCreated.CustomerName;
        }
        ...
    }
    ```
    
2. **Add Apply methods for each event it should handle**

    ```csharp
    public class Order
    {
        ...
    
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
    ```

3. **Add an assembly level attribute to opt in to source-generated instantiation of that object from a given stream.**

    ```csharp
   [assembly: CustomEventSourcing.AllowConstructionFromEvents(typeof(CustomEventSourcing.Procurement.Order))]
   ```
   
    _Optionally_ you can self-register your own delegate that can build the object from a stream:

    ```csharp
   EventSourceFactory.Register<Order>(BuildFromEvents_Order.Build);
    ```
   
4. **Load the Stream and them build the object from the stream**

    ```csharp
   var orderId = Guid.NewGuid();
    var stream = await store.LoadStreamAsync(orderId);
    stream.AddEvent(new OrderCreated(orderId, "Acme, Inc"));
    stream.AddEvent(new ItemAdded("Demand Forecasting Best Practices", 34.99m));
    stream.AddEvent(new ItemAdded("Implementing Domain Driven Design", 54.99m));
    stream.AddEvent(new OrderSubmitted());
    var order = EventSourceFactory.Build<Order>(stream.Events);

    ```
   

## TODO

1. Some helpers around using the "Decider Pattern" to handle commands and add events.