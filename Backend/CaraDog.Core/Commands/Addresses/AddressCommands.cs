using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Addresses;

namespace CaraDog.Core.Commands.Addresses;

public sealed record GetAddressesQuery() : ICommand<IReadOnlyList<AddressDto>>;

public sealed class GetAddressesHandler : ICommandHandler<GetAddressesQuery, IReadOnlyList<AddressDto>>
{
    private readonly IAddressService _service;

    public GetAddressesHandler(IAddressService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<AddressDto>> HandleAsync(GetAddressesQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetAddressByIdQuery(Guid Id) : ICommand<AddressDto>;

public sealed class GetAddressByIdHandler : ICommandHandler<GetAddressByIdQuery, AddressDto>
{
    private readonly IAddressService _service;

    public GetAddressByIdHandler(IAddressService service)
    {
        _service = service;
    }

    public Task<AddressDto> HandleAsync(GetAddressByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record CreateAddressCommand(AddressCreateRequest Request) : ICommand<AddressDto>;

public sealed class CreateAddressHandler : ICommandHandler<CreateAddressCommand, AddressDto>
{
    private readonly IAddressService _service;

    public CreateAddressHandler(IAddressService service)
    {
        _service = service;
    }

    public Task<AddressDto> HandleAsync(CreateAddressCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateAddressCommand(Guid Id, AddressUpdateRequest Request) : ICommand<AddressDto>;

public sealed class UpdateAddressHandler : ICommandHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IAddressService _service;

    public UpdateAddressHandler(IAddressService service)
    {
        _service = service;
    }

    public Task<AddressDto> HandleAsync(UpdateAddressCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteAddressCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteAddressHandler : ICommandHandler<DeleteAddressCommand, bool>
{
    private readonly IAddressService _service;

    public DeleteAddressHandler(IAddressService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteAddressCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
