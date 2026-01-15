using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Categories;

namespace CaraDog.Core.Commands.Categories;

public sealed record GetCategoriesQuery() : ICommand<IReadOnlyList<CategoryDto>>;

public sealed class GetCategoriesHandler : ICommandHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryService _service;

    public GetCategoriesHandler(ICategoryService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<CategoryDto>> HandleAsync(GetCategoriesQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(cancellationToken);
    }
}

public sealed record GetCategoryByIdQuery(Guid Id) : ICommand<CategoryDto>;

public sealed class GetCategoryByIdHandler : ICommandHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryService _service;

    public GetCategoryByIdHandler(ICategoryService service)
    {
        _service = service;
    }

    public Task<CategoryDto> HandleAsync(GetCategoryByIdQuery command, CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(command.Id, cancellationToken);
    }
}

public sealed record CreateCategoryCommand(CategoryCreateRequest Request) : ICommand<CategoryDto>;

public sealed class CreateCategoryHandler : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryService _service;

    public CreateCategoryHandler(ICategoryService service)
    {
        _service = service;
    }

    public Task<CategoryDto> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        return _service.CreateAsync(command.Request, cancellationToken);
    }
}

public sealed record UpdateCategoryCommand(Guid Id, CategoryUpdateRequest Request) : ICommand<CategoryDto>;

public sealed class UpdateCategoryHandler : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryService _service;

    public UpdateCategoryHandler(ICategoryService service)
    {
        _service = service;
    }

    public Task<CategoryDto> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(command.Id, command.Request, cancellationToken);
    }
}

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteCategoryHandler : ICommandHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryService _service;

    public DeleteCategoryHandler(ICategoryService service)
    {
        _service = service;
    }

    public async Task<bool> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(command.Id, cancellationToken);
        return true;
    }
}
