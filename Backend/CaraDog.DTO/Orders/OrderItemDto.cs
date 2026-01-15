namespace CaraDog.DTO.Orders;

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitNetPrice,
    decimal LineNet,
    decimal TaxAmount,
    decimal LineTotalGross
);
