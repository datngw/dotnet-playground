namespace HATEOAS.Features.Products;

public interface IProductService
{
  Task<List<Product>> GetAllAsync();
  Task<Product?> GetByIdAsync(Guid id);
}

public sealed class ProductService : IProductService
{
  private readonly IProductRepository _repo;
  public ProductService(IProductRepository repo) => _repo = repo;
  public Task<List<Product>> GetAllAsync() => _repo.GetAllAsync();
  public Task<Product?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
}