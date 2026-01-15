using CaraDog.DTO.Enums;

namespace CaraDog.DTO.Orders;

public sealed record OrderStatusUpdateRequest(
    OrderStatus Status
);
