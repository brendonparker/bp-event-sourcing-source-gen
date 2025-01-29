using CustomEventSourcing;
using CustomEventSourcing.Lib.Internal;
using CustomEventSourcing.Procurement;

var host = Host.CreateApplicationBuilder(args);

host.Services.AddEventSourcing("Data Source=db.sqlite3");

var app = host.Build();

using var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<EventSourceDbContext>();
context.Database.EnsureDeleted();
context.Database.EnsureCreated();

var store = scope.ServiceProvider.GetRequiredService<IStore>();
var orderId = Guid.NewGuid();
var stream = await store.LoadStreamAsync(orderId);
stream.AddEvent(new OrderCreated(orderId, "Acme, Inc"));
stream.AddEvent(new ItemAdded("Demand Forecasting Best Practices", 34.99m, 1));
stream.AddEvent(new ItemAdded("Implementing Domain Driven Design", 54.99m, 1));
stream.AddEvent(new OrderSubmitted());
await store.SaveChangesAsync();