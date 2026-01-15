using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Inventory;
using CaraDog.DTO.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly ICommandHandler<GetInventoriesQuery, IReadOnlyList<InventoryDto>> _getAllHandler;
    private readonly ICommandHandler<GetInventoryByIdQuery, InventoryDto> _getByIdHandler;
    private readonly ICommandHandler<GetInventoryByProductIdQuery, InventoryDto> _getByProductIdHandler;
    private readonly ICommandHandler<CreateInventoryCommand, InventoryDto> _createHandler;
    private readonly ICommandHandler<UpdateInventoryCommand, InventoryDto> _updateHandler;
    private readonly ICommandHandler<DeleteInventoryCommand, bool> _deleteHandler;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        ICommandHandler<GetInventoriesQuery, IReadOnlyList<InventoryDto>> getAllHandler,
        ICommandHandler<GetInventoryByIdQuery, InventoryDto> getByIdHandler,
        ICommandHandler<GetInventoryByProductIdQuery, InventoryDto> getByProductIdHandler,
        ICommandHandler<CreateInventoryCommand, InventoryDto> createHandler,
        ICommandHandler<UpdateInventoryCommand, InventoryDto> updateHandler,
        ICommandHandler<DeleteInventoryCommand, bool> deleteHandler,
        ILogger<InventoryController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getByProductIdHandler = getByProductIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-001 Get inventories");
        var result = await _getAllHandler.HandleAsync(new GetInventoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-002 Get inventory {InventoryId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetInventoryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<InventoryDto>> GetByProductId(Guid productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-003 Get inventory by product {ProductId}", productId);
        var result = await _getByProductIdHandler.HandleAsync(
            new GetInventoryByProductIdQuery(productId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> Create(InventoryCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-004 Create inventory");
        var result = await _createHandler.HandleAsync(new CreateInventoryCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InventoryDto>> Update(Guid id, InventoryUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-005 Update inventory {InventoryId}", id);
        var result = await _updateHandler.HandleAsync(new UpdateInventoryCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-INV-006 Delete inventory {InventoryId}", id);
        await _deleteHandler.HandleAsync(new DeleteInventoryCommand(id), cancellationToken);
        return NoContent();
    }
}
