using BFF.Shared.DTOs;

namespace BFF.Web.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(int id);
    Task<List<UserDto>> GetAllUsersAsync();
}
