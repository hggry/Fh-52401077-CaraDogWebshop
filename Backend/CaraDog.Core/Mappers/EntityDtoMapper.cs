using CaraDog.Db.Entities;
using DbOrderStatus = CaraDog.Db.Enums.OrderStatus;
using DbPaymentProvider = CaraDog.Db.Enums.PaymentProvider;
using CaraDog.DTO.Addresses;
using CaraDog.DTO.Categories;
using CaraDog.DTO.Customers;
using DtoOrderStatus = CaraDog.DTO.Enums.OrderStatus;
using DtoPaymentProvider = CaraDog.DTO.Enums.PaymentProvider;
using CaraDog.DTO.Inventory;
using CaraDog.DTO.Orders;
using CaraDog.DTO.Products;

namespace CaraDog.Core.Mappers;

public static class EntityDtoMapper
{
    public static ProductDto ToDto(this Product product, decimal taxRate)
    {
        var gross = Math.Round(product.NetPrice * (1 + taxRate), 2, MidpointRounding.AwayFromZero);
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.NetPrice,
            gross,
            taxRate,
            product.IsSoldOut,
            product.CategoryId,
            product.Category.Name);
    }

    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.CreatedAt);
    }

    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            address.Id,
            address.Street,
            address.City,
            address.PostalCode,
            address.CountryCode,
            address.State);
    }

    public static InventoryDto ToDto(this Inventory inventory)
    {
        return new InventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.Quantity,
            inventory.UpdatedAt);
    }

    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id,
            order.Customer.ToDto(),
            order.ShippingAddress.ToDto(),
            order.Items.Select(ToDto).ToList(),
            MapStatus(order.Status),
            MapProvider(order.PaymentProvider),
            order.SubtotalNet,
            order.TaxAmount,
            order.ShippingCost,
            order.TotalGross,
            order.CreatedAt);
    }

    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto(
            item.ProductId,
            item.Product.Name,
            item.Quantity,
            item.UnitNetPrice,
            item.LineNet,
            item.TaxAmount,
            item.LineTotalGross);
    }

    public static DtoOrderStatus MapStatus(DbOrderStatus status)
    {
        return status switch
        {
            DbOrderStatus.Created => DtoOrderStatus.Created,
            DbOrderStatus.PaymentPending => DtoOrderStatus.PaymentPending,
            DbOrderStatus.Paid => DtoOrderStatus.Paid,
            DbOrderStatus.Cancelled => DtoOrderStatus.Cancelled,
            _ => DtoOrderStatus.Created
        };
    }

    public static DtoPaymentProvider MapProvider(DbPaymentProvider provider)
    {
        return provider switch
        {
            DbPaymentProvider.Paypal => DtoPaymentProvider.Paypal,
            DbPaymentProvider.Stripe => DtoPaymentProvider.Stripe,
            _ => DtoPaymentProvider.None
        };
    }

    public static DbOrderStatus MapStatusDto(DtoOrderStatus status)
    {
        return status switch
        {
            DtoOrderStatus.Created => DbOrderStatus.Created,
            DtoOrderStatus.PaymentPending => DbOrderStatus.PaymentPending,
            DtoOrderStatus.Paid => DbOrderStatus.Paid,
            DtoOrderStatus.Cancelled => DbOrderStatus.Cancelled,
            _ => DbOrderStatus.Created
        };
    }

    public static DbPaymentProvider MapProviderDto(DtoPaymentProvider provider)
    {
        return provider switch
        {
            DtoPaymentProvider.Paypal => DbPaymentProvider.Paypal,
            DtoPaymentProvider.Stripe => DbPaymentProvider.Stripe,
            _ => DbPaymentProvider.None
        };
    }
}
