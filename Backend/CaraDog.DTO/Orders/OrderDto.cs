using CaraDog.DTO.Addresses;
using CaraDog.DTO.Customers;
using CaraDog.DTO.Enums;

namespace CaraDog.DTO.Orders;

public sealed record OrderDto(
    Guid Id,
    CustomerDto Customer,
    AddressDto ShippingAddress,
    IReadOnlyList<OrderItemDto> Items,
    OrderStatus Status,
    PaymentProvider PaymentProvider,
    decimal SubtotalNet,
    decimal TaxAmount,
    decimal ShippingCost,
    decimal TotalGross,
    DateTime CreatedAt
);
