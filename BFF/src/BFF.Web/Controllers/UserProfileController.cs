using BFF.Web.Aggregators;
using BFF.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BFF.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IDashboardAggregator _aggregator;

    public UserProfileController(IDashboardAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    // WHY: The profile page needs different data than the dashboard.
    // This endpoint returns user info + order stats, not product recommendations.
    // The BFF creates endpoints per UI view, not per domain entity.
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(int userId)
    {
        var profile = await _aggregator.GetUserProfileAsync(userId);

        if (profile.Name == string.Empty)
            return NotFound($"User {userId} not found");

        return Ok(profile);
    }
}
