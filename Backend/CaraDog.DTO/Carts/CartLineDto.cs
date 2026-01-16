namespace CaraDog.DTO.Carts;

public sealed record CartLineDto(
    string Sku,
    string Name,
    int Quantity,
    decimal UnitNetPrice,
    decimal LineNet,
    decimal TaxAmount,
    decimal LineTotalGross
);
