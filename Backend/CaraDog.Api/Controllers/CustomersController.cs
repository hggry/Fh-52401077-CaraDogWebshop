using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Customers;
using CaraDog.DTO.Customers;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICommandHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>> _getAllHandler;
    private readonly ICommandHandler<GetCustomerByIdQuery, CustomerDto> _getByIdHandler;
    private readonly ICommandHandler<CreateCustomerCommand, CustomerDto> _createHandler;
    private readonly ICommandHandler<UpdateCustomerCommand, CustomerDto> _updateHandler;
    private readonly ICommandHandler<DeleteCustomerCommand, bool> _deleteHandler;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICommandHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>> getAllHandler,
        ICommandHandler<GetCustomerByIdQuery, CustomerDto> getByIdHandler,
        ICommandHandler<CreateCustomerCommand, CustomerDto> createHandler,
        ICommandHandler<UpdateCustomerCommand, CustomerDto> updateHandler,
        ICommandHandler<DeleteCustomerCommand, bool> deleteHandler,
        ILogger<CustomersController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CUS-001 Get customers");
        var result = await _getAllHandler.HandleAsync(new GetCustomersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CUS-002 Get customer {CustomerId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CUS-003 Create customer");
        var result = await _createHandler.HandleAsync(new CreateCustomerCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, CustomerUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CUS-004 Update customer {CustomerId}", id);
        var result = await _updateHandler.HandleAsync(new UpdateCustomerCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CUS-005 Delete customer {CustomerId}", id);
        await _deleteHandler.HandleAsync(new DeleteCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}
