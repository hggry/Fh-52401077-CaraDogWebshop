using CaraDog.DTO.Customers;
using CaraDog.DTO.Enums;

namespace CaraDog.DTO.Orders;

public sealed record OrderCreateRequest(
    CustomerCreateRequest Customer,
    IReadOnlyList<OrderItemCreateRequest> Items,
    PaymentProvider PaymentProvider
);
