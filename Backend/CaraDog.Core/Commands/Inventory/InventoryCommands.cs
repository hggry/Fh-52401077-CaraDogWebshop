using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Inventory;

namespace CaraDog.Core.Commands.Inventory;

public sealed record GetInventoriesQuery() : ICommand<IReadOnlyList<InventoryDto>>;

public sealed class GetInventoriesHandler : ICommandHandler<GetInventoriesQuery, IReadOnlyList<InventoryDto>>
{
    private readonly IInventoryService _service;

    public GetInventoriesHandler(IInventoryService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<InventoryDto>> HandleAsync(GetInventoriesQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetInventoryByIdQuery(Guid Id) : ICommand<InventoryDto>;

public sealed class GetInventoryByIdHandler : ICommandHandler<GetInventoryByIdQuery, InventoryDto>
{
    private readonly IInventoryService _service;

    public GetInventoryByIdHandler(IInventoryService service)
    {
        _service = service;
    }

    public Task<InventoryDto> HandleAsync(GetInventoryByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record GetInventoryByProductIdQuery(Guid ProductId) : ICommand<InventoryDto>;

public sealed class GetInventoryByProductIdHandler : ICommandHandler<GetInventoryByProductIdQuery, InventoryDto>
{
    private readonly IInventoryService _service;

    public GetInventoryByProductIdHandler(IInventoryService service)
    {
        _service = service;
    }

    public Task<InventoryDto> HandleAsync(GetInventoryByProductIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByProductIdAsync(command.ProductId, cancellationToken);
    }
}

public sealed record CreateInventoryCommand(InventoryCreateRequest Request) : ICommand<InventoryDto>;

public sealed class CreateInventoryHandler : ICommandHandler<CreateInventoryCommand, InventoryDto>
{
    private readonly IInventoryService _service;

    public CreateInventoryHandler(IInventoryService service)
    {
        _service = service;
    }

    public Task<InventoryDto> HandleAsync(CreateInventoryCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateInventoryCommand(Guid Id, InventoryUpdateRequest Request) : ICommand<InventoryDto>;

public sealed class UpdateInventoryHandler : ICommandHandler<UpdateInventoryCommand, InventoryDto>
{
    private readonly IInventoryService _service;

    public UpdateInventoryHandler(IInventoryService service)
    {
        _service = service;
    }

    public Task<InventoryDto> HandleAsync(UpdateInventoryCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteInventoryCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteInventoryHandler : ICommandHandler<DeleteInventoryCommand, bool>
{
    private readonly IInventoryService _service;

    public DeleteInventoryHandler(IInventoryService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteInventoryCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
