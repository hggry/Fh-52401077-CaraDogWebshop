using CaraDog.DTO.Inventory;

namespace CaraDog.Core.Abstractions.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryDto> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<InventoryDto> CreateAsync(InventoryCreateRequest request, CancellationToken cancellationToken = default);
    Task<InventoryDto> UpdateAsync(Guid id, InventoryUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
