using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Products;

namespace CaraDog.Core.Commands.Products;

public sealed record GetProductsQuery() : ICommand<IReadOnlyList<ProductDto>>;

public sealed class GetProductsHandler : ICommandHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductService _service;

    public GetProductsHandler(IProductService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<ProductDto>> HandleAsync(GetProductsQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetProductByIdQuery(Guid Id) : ICommand<ProductDto>;

public sealed class GetProductByIdHandler : ICommandHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductService _service;

    public GetProductByIdHandler(IProductService service)
    {
        _service = service;
    }

    public Task<ProductDto> HandleAsync(GetProductByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record CreateProductCommand(ProductCreateRequest Request) : ICommand<ProductDto>;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductService _service;

    public CreateProductHandler(IProductService service)
    {
        _service = service;
    }

    public Task<ProductDto> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateProductCommand(Guid Id, ProductUpdateRequest Request) : ICommand<ProductDto>;

public sealed class UpdateProductHandler : ICommandHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductService _service;

    public UpdateProductHandler(IProductService service)
    {
        _service = service;
    }

    public Task<ProductDto> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteProductCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteProductHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductService _service;

    public DeleteProductHandler(IProductService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
