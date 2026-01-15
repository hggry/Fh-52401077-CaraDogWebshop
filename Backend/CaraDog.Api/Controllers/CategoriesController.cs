using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Categories;
using CaraDog.DTO.Categories;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICommandHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>> _getAllHandler;
    private readonly ICommandHandler<GetCategoryByIdQuery, CategoryDto> _getByIdHandler;
    private readonly ICommandHandler<CreateCategoryCommand, CategoryDto> _createHandler;
    private readonly ICommandHandler<UpdateCategoryCommand, CategoryDto> _updateHandler;
    private readonly ICommandHandler<DeleteCategoryCommand, bool> _deleteHandler;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICommandHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>> getAllHandler,
        ICommandHandler<GetCategoryByIdQuery, CategoryDto> getByIdHandler,
        ICommandHandler<CreateCategoryCommand, CategoryDto> createHandler,
        ICommandHandler<UpdateCategoryCommand, CategoryDto> updateHandler,
        ICommandHandler<DeleteCategoryCommand, bool> deleteHandler,
        ILogger<CategoriesController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CAT-001 Get categories");
        var result = await _getAllHandler.HandleAsync(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CAT-002 Get category {CategoryId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetCategoryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CAT-003 Create category");
        var result = await _createHandler.HandleAsync(new CreateCategoryCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, CategoryUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CAT-004 Update category {CategoryId}", id);
        var result = await _updateHandler.HandleAsync(new UpdateCategoryCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CAT-005 Delete category {CategoryId}", id);
        await _deleteHandler.HandleAsync(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
