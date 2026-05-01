using UserService.Models;

namespace UserService.Data;

public class UserStore
{
    private readonly List<User> _users =
    [
        new() { Id = 1, Username = "johndoe", Email = "john@example.com", FirstName = "John", LastName = "Doe", CreatedAt = new DateTime(2024, 1, 15), Role = "Customer", IsActive = true },
        new() { Id = 2, Username = "janedoe", Email = "jane@example.com", FirstName = "Jane", LastName = "Doe", CreatedAt = new DateTime(2024, 3, 22), Role = "Customer", IsActive = true },
        new() { Id = 3, Username = "admin", Email = "admin@example.com", FirstName = "Admin", LastName = "User", CreatedAt = new DateTime(2023, 6, 1), Role = "Admin", IsActive = true },
        new() { Id = 4, Username = "bobsmith", Email = "bob@example.com", FirstName = "Bob", LastName = "Smith", CreatedAt = new DateTime(2024, 7, 10), Role = "Customer", IsActive = false },
        new() { Id = 5, Username = "alicejones", Email = "alice@example.com", FirstName = "Alice", LastName = "Jones", CreatedAt = new DateTime(2024, 9, 5), Role = "Customer", IsActive = true },
    ];

    public Task<List<User>> GetAllAsync() => Task.FromResult(_users);

    public Task<User?> GetByIdAsync(int id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
}
