// =====================================================================
// BFF.Mobile — Backend for Frontend (Mobile)
// =====================================================================
// WHY a separate BFF for mobile?
// Different frontends have different needs:
//   - Web: larger payloads, more data, richer responses
//   - Mobile: smaller payloads, aggressive caching, fewer requests
//
// This BFF uses Minimal APIs (lighter weight) and returns compact responses.
// Compare its endpoints with BFF.Web to see how the same backend services
// serve different frontend needs.
// =====================================================================

using BFF.Mobile.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var userServiceUrl = builder.Configuration["ServiceUrls:UserService"] ?? "http://localhost:5001";
var orderServiceUrl = builder.Configuration["ServiceUrls:OrderService"] ?? "http://localhost:5002";
var productServiceUrl = builder.Configuration["ServiceUrls:ProductService"] ?? "http://localhost:5003";

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri(orderServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(productServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDashboardEndpoints();
app.MapOrderEndpoints();

app.Run();
