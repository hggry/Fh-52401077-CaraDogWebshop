using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using CaraDog.DTO.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly CaraDogDbContext _dbContext;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(CaraDogDbContext dbContext, ILogger<CategoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return categories.Select(category => category.ToDto()).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category {id} was not found.");
        }

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var exists = await _dbContext.Categories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Category {request.Name} already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CAT-001 Category created {CategoryId}", category.Id);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CategoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category {id} was not found.");
        }

        var nameExists = await _dbContext.Categories
            .AnyAsync(c => c.Name == request.Name && c.Id != id, cancellationToken);

        if (nameExists)
        {
            throw new ConflictException($"Category {request.Name} already exists.");
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CAT-002 Category updated {CategoryId}", category.Id);

        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException($"Category {id} was not found.");
        }

        var inUse = await _dbContext.Products
            .AnyAsync(p => p.CategoryId == id, cancellationToken);

        if (inUse)
        {
            throw new ConflictException($"Category {id} is referenced by products and cannot be deleted.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CAT-003 Category deleted {CategoryId}", id);
    }

    private static void ValidateRequest(CategoryCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Category name is required.");
        }
    }

    private static void ValidateRequest(CategoryUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Category name is required.");
        }
    }
}
