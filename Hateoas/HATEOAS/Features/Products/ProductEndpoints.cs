using HATEOAS.Common;

namespace HATEOAS.Features.Products;

public static class ProductEndpoints
{
  public static void MapProductEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("/products", async (IProductService service, ILinkBuilder<Product> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var products = await service.GetAllAsync();
      return Results.Ok(products.Select(p => builder.Build(p, linker, ctx)));
    }).WithName("GetAllProducts");

    app.MapGet("/products/{id}", async (Guid id, IProductService service, ILinkBuilder<Product> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var product = await service.GetByIdAsync(id);
      return product is null ? Results.NotFound() : Results.Ok(builder.Build(product, linker, ctx));
    }).WithName("GetProductById");
  }
}
