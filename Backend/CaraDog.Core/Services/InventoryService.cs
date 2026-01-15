using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using CaraDog.DTO.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly CaraDogDbContext _dbContext;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(CaraDogDbContext dbContext, ILogger<InventoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InventoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var inventories = await _dbContext.Inventories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return inventories.Select(inventory => inventory.ToDto()).ToList();
    }

    public async Task<InventoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await _dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (inventory is null)
        {
            throw new NotFoundException($"Inventory {id} was not found.");
        }

        return inventory.ToDto();
    }

    public async Task<InventoryDto> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var inventory = await _dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);

        if (inventory is null)
        {
            throw new NotFoundException($"Inventory for product {productId} was not found.");
        }

        return inventory.ToDto();
    }

    public async Task<InventoryDto> CreateAsync(InventoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 0)
        {
            throw new ValidationException("Inventory quantity must be greater than or equal to zero.");
        }

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ValidationException($"Product {request.ProductId} does not exist.");
        }

        var exists = await _dbContext.Inventories
            .AnyAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Inventory for product {request.ProductId} already exists.");
        }

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UpdatedAt = DateTime.UtcNow
        };

        product.IsSoldOut = request.Quantity <= 0;

        _dbContext.Inventories.Add(inventory);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-INV-001 Inventory created {InventoryId}", inventory.Id);

        return inventory.ToDto();
    }

    public async Task<InventoryDto> UpdateAsync(Guid id, InventoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 0)
        {
            throw new ValidationException("Inventory quantity must be greater than or equal to zero.");
        }

        var inventory = await _dbContext.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (inventory is null)
        {
            throw new NotFoundException($"Inventory {id} was not found.");
        }

        inventory.Quantity = request.Quantity;
        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Product.IsSoldOut = request.Quantity <= 0;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-INV-002 Inventory updated {InventoryId}", inventory.Id);

        return inventory.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await _dbContext.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (inventory is null)
        {
            throw new NotFoundException($"Inventory {id} was not found.");
        }

        inventory.Product.IsSoldOut = true;
        _dbContext.Inventories.Remove(inventory);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-INV-003 Inventory deleted {InventoryId}", id);
    }
}
