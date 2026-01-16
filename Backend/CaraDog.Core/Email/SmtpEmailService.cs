using System.Net;
using System.Net.Mail;
using CaraDog.Core.Abstractions.Email;
using CaraDog.DTO.Orders;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(EmailSettings settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            _logger.LogInformation(
                "HBH-EML-002 Email not configured, skipping send for OrderId {OrderId}",
                order.Id);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = $"CaraDog Bestellbestätigung {order.Id}",
            Body = $"Danke für deine Bestellung. Bestellnummer: {order.Id}. Gesamt: {order.TotalGross:0.00} EUR.",
            IsBodyHtml = false
        };
        message.To.Add(order.Customer.Email);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUser))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword);
        }

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation(
            "HBH-EML-001 Order confirmation sent for OrderId {OrderId} to {Email}",
            order.Id,
            order.Customer.Email);
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_settings.SmtpHost)
               && !string.IsNullOrWhiteSpace(_settings.FromEmail);
    }
}
