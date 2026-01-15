using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Products;
using CaraDog.DTO.Products;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ICommandHandler<GetProductsQuery, IReadOnlyList<ProductDto>> _getAllHandler;
    private readonly ICommandHandler<GetProductByIdQuery, ProductDto> _getByIdHandler;
    private readonly ICommandHandler<CreateProductCommand, ProductDto> _createHandler;
    private readonly ICommandHandler<UpdateProductCommand, ProductDto> _updateHandler;
    private readonly ICommandHandler<DeleteProductCommand, bool> _deleteHandler;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ICommandHandler<GetProductsQuery, IReadOnlyList<ProductDto>> getAllHandler,
        ICommandHandler<GetProductByIdQuery, ProductDto> getByIdHandler,
        ICommandHandler<CreateProductCommand, ProductDto> createHandler,
        ICommandHandler<UpdateProductCommand, ProductDto> updateHandler,
        ICommandHandler<DeleteProductCommand, bool> deleteHandler,
        ILogger<ProductsController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-001 Get products");
        var result = await _getAllHandler.HandleAsync(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-002 Get product {ProductId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-003 Create product");
        var result = await _createHandler.HandleAsync(new CreateProductCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, ProductUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-004 Update product {ProductId}", id);
        var result = await _updateHandler.HandleAsync(new UpdateProductCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-005 Delete product {ProductId}", id);
        await _deleteHandler.HandleAsync(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
