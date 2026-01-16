using CaraDog.DTO.Carts;
using CaraDog.DTO.Orders;

namespace CaraDog.Core.Abstractions.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CartInfoDto> GetCartInfoAsync(CartInfoRequest request, CancellationToken cancellationToken = default);
}
