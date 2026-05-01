using ProductService.Models;

namespace ProductService.Data;

public class ProductStore
{
    private readonly List<Product> _products =
    [
        new() { Id = 1, Name = "Wireless Headphones", Description = "Premium noise-cancelling headphones", Price = 199.99m, Category = "Electronics", StockQuantity = 50, ImageUrl = "/images/headphones.jpg", IsActive = true },
        new() { Id = 2, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard with Cherry MX switches", Price = 449.99m, Category = "Electronics", StockQuantity = 30, ImageUrl = "/images/keyboard.jpg", IsActive = true },
        new() { Id = 3, Name = "Programming Book", Description = "Clean Code by Robert C. Martin", Price = 29.99m, Category = "Books", StockQuantity = 100, ImageUrl = "/images/book.jpg", IsActive = true },
        new() { Id = 4, Name = "Mouse Pad", Description = "Extended gaming mouse pad", Price = 59.99m, Category = "Electronics", StockQuantity = 200, ImageUrl = "/images/mousepad.jpg", IsActive = true },
        new() { Id = 5, Name = "T-Shirt", Description = "Developer themed cotton t-shirt", Price = 34.99m, Category = "Clothing", StockQuantity = 75, ImageUrl = "/images/tshirt.jpg", IsActive = true },
        new() { Id = 6, Name = "USB Hub", Description = "7-port USB 3.0 hub", Price = 79.99m, Category = "Electronics", StockQuantity = 0, ImageUrl = "/images/usbhub.jpg", IsActive = false },
        new() { Id = 7, Name = "Hoodie", Description = "Warm developer hoodie", Price = 39.99m, Category = "Clothing", StockQuantity = 40, ImageUrl = "/images/hoodie.jpg", IsActive = true },
        new() { Id = 8, Name = "Design Patterns Book", Description = "Gang of Four Design Patterns", Price = 44.99m, Category = "Books", StockQuantity = 60, ImageUrl = "/images/designbook.jpg", IsActive = true },
    ];

    public Task<List<Product>> GetAllAsync() =>
        Task.FromResult(_products.Where(p => p.IsActive).ToList());

    public Task<Product?> GetByIdAsync(int id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<List<Product>> GetByIdsAsync(int[] ids) =>
        Task.FromResult(_products.Where(p => ids.Contains(p.Id)).ToList());
}
