using HATEOAS.Common;

namespace HATEOAS.Features.Products;

public class ProductLinkBuilder : ILinkBuilder<Product>
{
  public Resource<Product> Build(Product product, LinkGenerator linker, HttpContext ctx)
  {
    var links = new List<Link>
    {
        new Link(linker.GetUriByName(ctx, "GetProductById", new { id = product.Id })!, "self", "GET")
    };

    return new Resource<Product>(product, links);
  }
}