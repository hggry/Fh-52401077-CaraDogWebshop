using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using CaraDog.DTO.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class ProductService : IProductService
{
    private readonly CaraDogDbContext _dbContext;
    private readonly ITaxCalculatorResolver _taxResolver;
    private readonly ILogger<ProductService> _logger;

    public ProductService(CaraDogDbContext dbContext, ITaxCalculatorResolver taxResolver, ILogger<ProductService> logger)
    {
        _dbContext = dbContext;
        _taxResolver = taxResolver;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var taxRate = GetCurrentTaxRate();
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return products.Select(product => product.ToDto(taxRate)).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var taxRate = GetCurrentTaxRate();
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product {id} was not found.");
        }

        return product.ToDto(taxRate);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException($"Category {request.CategoryId} does not exist.");
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(p => p.Sku == request.Sku, cancellationToken);

        if (skuExists)
        {
            throw new ConflictException($"SKU {request.Sku} already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Sku = request.Sku.Trim(),
            NetPrice = request.NetPrice,
            CategoryId = request.CategoryId,
            IsSoldOut = true
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-PRD-001 Product created {ProductId}", product.Id);

        product.Category = category;
        return product.ToDto(GetCurrentTaxRate());
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var product = await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product {id} was not found.");
        }

        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new ValidationException($"Category {request.CategoryId} does not exist.");
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(p => p.Sku == request.Sku && p.Id != id, cancellationToken);

        if (skuExists)
        {
            throw new ConflictException($"SKU {request.Sku} already exists.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Sku = request.Sku.Trim();
        product.NetPrice = request.NetPrice;
        product.CategoryId = request.CategoryId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-PRD-002 Product updated {ProductId}", product.Id);

        return product.ToDto(GetCurrentTaxRate());
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product {id} was not found.");
        }

        var usedInOrders = await _dbContext.OrderItems
            .AnyAsync(o => o.ProductId == id, cancellationToken);

        if (usedInOrders)
        {
            throw new ConflictException($"Product {id} is referenced by orders and cannot be deleted.");
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-PRD-003 Product deleted {ProductId}", id);
    }

    private static void ValidateRequest(ProductCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new ValidationException("SKU is required.");
        }

        if (request.NetPrice < 0)
        {
            throw new ValidationException("Net price must be greater than or equal to zero.");
        }
    }

    private static void ValidateRequest(ProductUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new ValidationException("SKU is required.");
        }

        if (request.NetPrice < 0)
        {
            throw new ValidationException("Net price must be greater than or equal to zero.");
        }
    }

    private decimal GetCurrentTaxRate()
    {
        // Umsatzgrenze: aktuell immer AT 20% verwenden, unabhängig vom Land.
        return _taxResolver.Resolve("AT").Rate;
    }
}
