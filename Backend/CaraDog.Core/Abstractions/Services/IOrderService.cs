using CaraDog.DTO.Orders;

namespace CaraDog.Core.Abstractions.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
