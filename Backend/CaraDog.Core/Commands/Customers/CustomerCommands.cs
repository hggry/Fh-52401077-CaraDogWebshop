using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Customers;

namespace CaraDog.Core.Commands.Customers;

public sealed record GetCustomersQuery() : ICommand<IReadOnlyList<CustomerDto>>;

public sealed class GetCustomersHandler : ICommandHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerService _service;

    public GetCustomersHandler(ICustomerService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<CustomerDto>> HandleAsync(GetCustomersQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetCustomerByIdQuery(Guid Id) : ICommand<CustomerDto>;

public sealed class GetCustomerByIdHandler : ICommandHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerService _service;

    public GetCustomerByIdHandler(ICustomerService service)
    {
        _service = service;
    }

    public Task<CustomerDto> HandleAsync(GetCustomerByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record CreateCustomerCommand(CustomerCreateRequest Request) : ICommand<CustomerDto>;

public sealed class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerService _service;

    public CreateCustomerHandler(ICustomerService service)
    {
        _service = service;
    }

    public Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateCustomerCommand(Guid Id, CustomerUpdateRequest Request) : ICommand<CustomerDto>;

public sealed class UpdateCustomerHandler : ICommandHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerService _service;

    public UpdateCustomerHandler(ICustomerService service)
    {
        _service = service;
    }

    public Task<CustomerDto> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteCustomerCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteCustomerHandler : ICommandHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerService _service;

    public DeleteCustomerHandler(ICustomerService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteCustomerCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
