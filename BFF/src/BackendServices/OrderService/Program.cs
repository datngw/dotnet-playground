using OrderService.Data;
using OrderService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/orders", async (int? userId, OrderStore store) =>
{
    var orders = userId.HasValue
        ? (await store.GetByUserIdAsync(userId.Value))
        : (await store.GetAllAsync());
    return Results.Ok(orders);
});

app.MapGet("/api/orders/{id}", async (int id, OrderStore store) =>
    await store.GetByIdAsync(id) is { } order
        ? Results.Ok(order)
        : Results.NotFound());

app.MapGet("/api/orders/user/{userId}", async (int userId, OrderStore store) =>
    Results.Ok(await store.GetByUserIdAsync(userId)));

app.MapPost("/api/orders", async (Order order, OrderStore store) =>
{
    // In a real service, this would persist to a database
    return Results.Created($"/api/orders/{order.Id}", order);
});

app.Run();
