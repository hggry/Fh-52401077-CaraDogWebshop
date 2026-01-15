using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Commands.Addresses;
using CaraDog.DTO.Addresses;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/addresses")]
public sealed class AddressesController : ControllerBase
{
    private readonly ICommandHandler<GetAddressesQuery, IReadOnlyList<AddressDto>> _getAllHandler;
    private readonly ICommandHandler<GetAddressByIdQuery, AddressDto> _getByIdHandler;
    private readonly ICommandHandler<CreateAddressCommand, AddressDto> _createHandler;
    private readonly ICommandHandler<UpdateAddressCommand, AddressDto> _updateHandler;
    private readonly ICommandHandler<DeleteAddressCommand, bool> _deleteHandler;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        ICommandHandler<GetAddressesQuery, IReadOnlyList<AddressDto>> getAllHandler,
        ICommandHandler<GetAddressByIdQuery, AddressDto> getByIdHandler,
        ICommandHandler<CreateAddressCommand, AddressDto> createHandler,
        ICommandHandler<UpdateAddressCommand, AddressDto> updateHandler,
        ICommandHandler<DeleteAddressCommand, bool> deleteHandler,
        ILogger<AddressesController> logger)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ADR-001 Get addresses");
        var result = await _getAllHandler.HandleAsync(new GetAddressesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ADR-002 Get address {AddressId}", id);
        var result = await _getByIdHandler.HandleAsync(new GetAddressByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(AddressCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ADR-003 Create address");
        var result = await _createHandler.HandleAsync(new CreateAddressCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(Guid id, AddressUpdateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ADR-004 Update address {AddressId}", id);
        var result = await _updateHandler.HandleAsync(new UpdateAddressCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-ADR-005 Delete address {AddressId}", id);
        await _deleteHandler.HandleAsync(new DeleteAddressCommand(id), cancellationToken);
        return NoContent();
    }
}
