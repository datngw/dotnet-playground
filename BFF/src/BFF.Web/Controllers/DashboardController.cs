using BFF.Web.Aggregators;
using BFF.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BFF.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardAggregator _aggregator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardAggregator aggregator, ILogger<DashboardController> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    // WHY: A single endpoint replaces what would be 3+ frontend calls.
    // GET /api/dashboard/1 → aggregated data from UserService + OrderService + ProductService
    [HttpGet("{userId}")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(int userId)
    {
        _logger.LogInformation("Dashboard request for user {UserId}", userId);

        var dashboard = await _aggregator.GetDashboardAsync(userId);

        if (dashboard.User.Name == string.Empty)
            return NotFound($"User {userId} not found");

        return Ok(dashboard);
    }
}
