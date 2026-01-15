using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Orders;
using CaraDog.DTO.Orders;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ICommandHandler<GetOrdersQuery, IReadOnlyList<OrderDto>> _getAllHandler;
    private readonly ICommandHandler<GetOrderByIdQuery, OrderDto> _getByIdHandler;
    private readonly ICommandHandler<CreateOrderCommand, OrderDto> _createHandler;
    private readonly ICommandHandler<UpdateOrderStatusCommand, OrderDto> _updateStatusHandler;
    private readonly ICommandHandler<DeleteOrderCommand, bool> _deleteHandler;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        ICommandHandler<GetOrdersQuery, IReadOnlyList<OrderDto>> getAllHandler,
        ICommandHandler<GetOrderByIdQuery, OrderDto> getByIdHandler,
        ICommandHandler<CreateOrderCommand, OrderDto> createHandler,
        ICommandHandler<UpdateOrderStatusCommand, OrderDto> updateStatusHandler,
        ICommandHandler<DeleteOrderCommand, bool> deleteHandler,
        ILogger<OrdersController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateStatusHandler = updateStatusHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-001 Get orders");
        var result = await _getAllHandler.HandleAsync(new GetOrdersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-002 Get order {OrderId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-003 Create order");
        var result = await _createHandler.HandleAsync(new CreateOrderCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-004 Update order status {OrderId}", id);
        var result = await _updateStatusHandler.HandleAsync(new UpdateOrderStatusCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ORD-005 Delete order {OrderId}", id);
        await _deleteHandler.HandleAsync(new DeleteOrderCommand(id), cancellationToken);
        return NoContent();
    }
}
