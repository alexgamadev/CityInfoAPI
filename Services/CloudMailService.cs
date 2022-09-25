namespace CityInfo.API.Services;

public class CloudMailService : IMailService
{
    private readonly string _mailTo = string.Empty;
    private readonly string _mailFrom = string.Empty;

    public CloudMailService(IConfiguration configuration)
    {
        _mailFrom = configuration["MailSettings:MailFromAddress"];
        _mailTo = configuration["MailSettings:MailToAddress"];
    }

    public void Send(string subject, string message)
    {
        // Just outputs to console window
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, " + $"with {nameof(CloudMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}
