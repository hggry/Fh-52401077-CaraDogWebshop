using CaraDog.DTO.Addresses;
using CaraDog.DTO.Customers;
using CaraDog.DTO.Enums;

namespace CaraDog.DTO.Orders;

public sealed record OrderCreateRequest(
    CustomerCreateRequest Customer,
    AddressCreateRequest ShippingAddress,
    IReadOnlyList<OrderItemCreateRequest> Items,
    PaymentProvider PaymentProvider
);
