namespace CaraDog.Core.Email;

public sealed class EmailSettings
{
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string SmtpUser { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string FromName { get; init; } = "CaraDog";
    public string FromEmail { get; init; } = "noreply@caradog.local";
    public bool EnableSsl { get; init; } = true;
}
