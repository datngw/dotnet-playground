// In production, each BFF would define its own DTOs rather than sharing a library.
// This shared project is for learning convenience. The BFF owns its response shapes
// and should map from raw JSON — not depend on backend service types.
namespace BFF.Shared.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
