namespace HATEOAS.Features.Products;

public interface IProductRepository
{
  Task<Product?> GetByIdAsync(Guid id);
  Task<List<Product>> GetAllAsync();
}
