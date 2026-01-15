using CaraDog.Core.Abstractions.Email;
using CaraDog.DTO.Orders;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Email;

public sealed class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderConfirmationAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "HBH-EML-001 Order confirmation queued for OrderId {OrderId} to {Email}",
            order.Id,
            order.Customer.Email);

        return Task.CompletedTask;
    }
}
