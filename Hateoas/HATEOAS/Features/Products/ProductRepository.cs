namespace HATEOAS.Features.Products;

public class ProductRepository : IProductRepository
{
  private readonly List<Product> _products = new()
    {
        new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 999.99M },
        new Product { Id = Guid.NewGuid(), Name = "Smartphone", Price = 499.99M },
        new Product { Id = Guid.NewGuid(), Name = "Tablet", Price = 299.99M }
    };

  public Task<List<Product>> GetAllAsync() => Task.FromResult(_products.ToList());
  public Task<Product?> GetByIdAsync(Guid id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
}