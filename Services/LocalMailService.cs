namespace CityInfo.API.Services;

public class LocalMailService : IMailService
{
    private readonly string _mailTo = string.Empty;
    private readonly string _mailFrom = string.Empty;

    public LocalMailService(IConfiguration configuration)
    {
        _mailFrom = configuration["MailSettings:MailFromAddress"];
        _mailTo = configuration["MailSettings:MailToAddress"];
    }

    public void Send(string subject, string message)
    {
        // Just outputs to console window
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, " + $"with {nameof(LocalMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}
