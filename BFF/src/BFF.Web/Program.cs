// =====================================================================
// BFF.Web — Backend for Frontend (Web)
// =====================================================================
// WHY A BFF?
// In a microservices architecture, a web frontend would need to make
// multiple HTTP calls to different services to render a single page.
// This creates problems:
//   1. Multiple round trips = slow page loads
//   2. Frontend must know about every microservice URL
//   3. Frontend must handle each service's error format differently
//   4. No central place for cross-cutting concerns (caching, auth)
//
// The BFF solves this by sitting between the frontend and microservices,
// aggregating calls and tailoring responses for the Web frontend.
// =====================================================================

using BFF.Web.Aggregators;
using BFF.Web.Mapping;
using BFF.Web.Middleware;
using BFF.Web.Services;
using BFF.Web.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Step 1: Basic Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Step 2: HttpClient Factory with Resilience ---
// Each downstream service gets its own named HttpClient.
// AddStandardResilienceHandler adds retry (3x exponential backoff),
// circuit breaker (50% failure over 30s → break for 30s), and timeout (10s).
// IHttpClientFactory prevents socket exhaustion (a common HttpClient bug).
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

// --- Step 3: Response Caching ---
builder.Services.AddMemoryCache();

// --- Step 4: Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("BffRateLimit", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(60);
    });
    options.RejectionStatusCode = 429;
});

// --- Step 5: Application Services ---
builder.Services.AddScoped<IUserService, UserBffService>();
builder.Services.AddScoped<IOrderService, OrderBffService>();
builder.Services.AddScoped<IProductService, ProductBffService>();
builder.Services.AddScoped<IDashboardAggregator, DashboardAggregator>();
builder.Services.AddScoped<ResponseMapper>();

// --- Middleware ---
builder.Services.AddSingleton<RequestLoggingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
