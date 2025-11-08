namespace HATEOAS.Features.Products;

public sealed class Product
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Name { get; set; } = string.Empty;
  public decimal Price { get; set; }
}