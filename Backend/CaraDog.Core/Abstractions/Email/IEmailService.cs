using CaraDog.DTO.Orders;

namespace CaraDog.Core.Abstractions.Email;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(OrderDto order, CancellationToken cancellationToken = default);
}
