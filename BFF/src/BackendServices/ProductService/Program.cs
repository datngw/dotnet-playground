using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ProductStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/products", async (string? category, ProductStore store) =>
{
    var products = await store.GetAllAsync();
    if (category is not null)
        products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", async (int id, ProductStore store) =>
    await store.GetByIdAsync(id) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

// Batch fetch — the BFF uses this to enrich order items with product details
app.MapGet("/api/products/batch", async (string ids, ProductStore store) =>
{
    var idArray = ids.Split(',').Select(int.Parse).ToArray();
    return Results.Ok(await store.GetByIdsAsync(idArray));
});

app.Run();
