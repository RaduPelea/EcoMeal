namespace EcoMeal.api.Infrastructure;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
}

// Sends real emails over SMTP using the "Email" section from appsettings.
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var host = _config["Email:Host"];
        var port = int.TryParse(_config["Email:Port"], out var p) ? p : 587;
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var from = _config["Email:From"] ?? username;

        using var client = new System.Net.Mail.SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new System.Net.NetworkCredential(username, password)
        };

        using var message = new System.Net.Mail.MailMessage(from!, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}

// Development stand-in: logs the email instead of sending it.
// Used automatically while Email:Host is empty in appsettings.
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("EMAIL (dev, not sent) To: {To} | Subject: {Subject} | Body: {Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
