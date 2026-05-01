using System.Net;
using BFF.Shared.DTOs;
using BFF.Web.Services.Interfaces;

namespace BFF.Web.Services;

// WHY: Each BFF service wraps one downstream microservice via HttpClient.
// Using IHttpClientFactory prevents socket exhaustion — a common bug where
// HttpClient instances are created/disposed too quickly, exhausting ports.
public class UserBffService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserBffService> _logger;

    public UserBffService(IHttpClientFactory httpClientFactory, ILogger<UserBffService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("UserService");
        _logger = logger;
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        _logger.LogInformation("BFF calling UserService: GET /api/users/{Id}", id);

        var response = await _httpClient.GetAsync($"/api/users/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("BFF calling UserService: GET /api/users");

        var response = await _httpClient.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? [];
    }
}
