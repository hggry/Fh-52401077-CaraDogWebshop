using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Orders;

namespace CaraDog.Core.Commands.Orders;

public sealed record GetOrdersQuery() : ICommand<IReadOnlyList<OrderDto>>;

public sealed class GetOrdersHandler : ICommandHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderService _service;

    public GetOrdersHandler(IOrderService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<OrderDto>> HandleAsync(GetOrdersQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetOrderByIdQuery(Guid Id) : ICommand<OrderDto>;

public sealed class GetOrderByIdHandler : ICommandHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderService _service;

    public GetOrderByIdHandler(IOrderService service)
    {
        _service = service;
    }

    public Task<OrderDto> HandleAsync(GetOrderByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record CreateOrderCommand(OrderCreateRequest Request) : ICommand<OrderDto>;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderService _service;

    public CreateOrderHandler(IOrderService service)
    {
        _service = service;
    }

    public Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateOrderStatusCommand(Guid Id, OrderStatusUpdateRequest Request) : ICommand<OrderDto>;

public sealed class UpdateOrderStatusHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderService _service;

    public UpdateOrderStatusHandler(IOrderService service)
    {
        _service = service;
    }

    public Task<OrderDto> HandleAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateStatusAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteOrderCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteOrderHandler : ICommandHandler<DeleteOrderCommand, bool>
{
    private readonly IOrderService _service;

    public DeleteOrderHandler(IOrderService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
