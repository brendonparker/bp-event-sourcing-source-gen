using System.Collections.Concurrent;
using CustomEventSourcing;
using CustomEventSourcing.Lib;
using CustomEventSourcing.Procurement;

var host = Host.CreateApplicationBuilder(args);
host.Services.AddEventSourcing("Data Source=db.sqlite3");
var app = host.Build();

using var scope = app.Services.CreateScope();

var setup = scope.ServiceProvider.GetRequiredService<IDatabaseSetup>();
setup.EnsureDeleted();
setup.EnsureCreated();

var store = scope.ServiceProvider.GetRequiredService<IStore>();
var orderId = Guid.NewGuid();
var stream = await store.LoadStreamAsync(orderId);
stream.AddEvent(new OrderCreated(orderId, "Acme, Inc"));
stream.AddEvent(new ItemAdded("Demand Forecasting Best Practices", 34.99m, 1));
stream.AddEvent(new ItemAdded("Implementing Domain Driven Design", 54.99m, 1));
stream.AddEvent(new OrderSubmitted());

// This is how you might use reflection, bleh
// var order = stream.ConstructFromStream<Order>();

// Instead, we'll use source-generators :)

// Could register your own factory
// EventSourceFactory.Register<Order>(BuildFromEvents_Order.Build);

// Then can construct your object from the event stream
var order = EventSourceFactory.Build<Order>(stream.Events);

Console.WriteLine($"{order.Id} {order.CustomerName}");
await store.SaveChangesAsync();