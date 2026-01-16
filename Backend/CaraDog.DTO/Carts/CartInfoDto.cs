namespace CaraDog.DTO.Carts;

public sealed record CartInfoDto(
    IReadOnlyList<CartLineDto> Items,
    decimal SubtotalNet,
    decimal TaxAmount,
    decimal ShippingCost,
    decimal TotalGross
);
