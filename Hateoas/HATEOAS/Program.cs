using HATEOAS.Common;
using HATEOAS.Features.Orders;
using HATEOAS.Features.Products;

var builder = WebApplication.CreateBuilder(args);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILinkBuilder<Product>, ProductLinkBuilder>();

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILinkBuilder<Order>, OrderLinkBuilder>();

var app = builder.Build();

// Enable swagger in Development environment (or always if you prefer)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HATEOAS API V1");
    });
}

// Map endpoints
app.MapProductEndpoints();
app.MapOrderEndpoints();

app.Run();