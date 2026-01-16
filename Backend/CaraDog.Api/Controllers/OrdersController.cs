using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Orders;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-001 Create order");
        var result = await _orderService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-002 Update order status {OrderId}", id);
        var result = await _orderService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-003 Get order {OrderId}", id);
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}
