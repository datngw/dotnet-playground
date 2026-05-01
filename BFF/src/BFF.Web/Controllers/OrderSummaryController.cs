using BFF.Web.Aggregators;
using BFF.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BFF.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderDetailController : ControllerBase
{
    private readonly IDashboardAggregator _aggregator;

    public OrderDetailController(IDashboardAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    // WHY: Enrichment pattern — the OrderService only stores ProductId.
    // The BFF fetches product details from ProductService to enrich the response
    // with product names and images. The frontend never calls ProductService directly.
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDetailResponse>> GetOrderDetail(int orderId)
    {
        var order = await _aggregator.GetOrderDetailAsync(orderId);

        if (order is null)
            return NotFound($"Order {orderId} not found");

        return Ok(order);
    }
}
