using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class NotificationService
{
    private readonly NotificationsConfig _config;
    private readonly ILogger<NotificationService> _logger;
    public NotificationService(NotificationsConfig notificationsConfig, ILogger<NotificationService> logger)
    {
        _config = notificationsConfig;
        _logger = logger;
    }

    public async Task SendPersonNotification(Person person, NotificationTemplate template)
    {
        try
        {
            await SendPersonEmail(person, template);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send notification to person {personId}: {message}", person.Id, ex.Message);
        }
    }

    private async Task SendPersonEmail(Person person, NotificationTemplate template)
    {
        if (!_config.Email.IsEnabled)
        {
            _logger.LogInformation("Skip notification as email notifications are disabled");
            return;
        }
        if (person.Email is null)
        {
            _logger.LogInformation("Skip notification as person {personId} does not have an email address", person.Id);
            return;
        }

        var templateData = GetEmailTemplateData(person, template);

        var client = new AmazonSimpleEmailServiceClient();
        var sendRequest = new SendEmailRequest
        {
            Source = _config.Email.FromAddress,
            Destination = new Destination
            {
                ToAddresses = new List<string> { person.Email }
            },
            Message = new Message
            {
                Subject = new Content(templateData.Subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = templateData.Body
                    }
                }
            },
        };

        await client.SendEmailAsync(sendRequest);
    }

    private static EmailTemplate GetEmailTemplateData(Person person, NotificationTemplate template)
    {
        var personFirstName = person.GivenName ?? person.Email;
        return template switch
        {
            NotificationTemplate.AccountToBeDeletedFromInactivity => new EmailTemplate
            {
                Subject = "Your Access Finland account will be deleted from inactivity!",
                Body = @$"
                        <h1>Your Access Finland account will be deleted from inactivity!</h1>
                        <p>Hi {personFirstName}, you have not been active in Access Finland for a long time.</p>
                        <p>Unless you log in to Access Finland within a month, your account will be automatically deleted.</p>
                        <p>If you want to keep your account, please log in to Access Finland.</p>
                    "
            },
            NotificationTemplate.AccountDeletedFromInactivity => new EmailTemplate
            {
                Subject = "Your Access Finland account was deleted from inactivity!",
                Body = @$"
                        <h1>Your Access Finland account was deleted from inactivity!</h1>
                        <p>Hi {personFirstName}, a month ago we sent you an email about your Access Finland account being deleted from inactivity.</p>
                        <p>Since you did not log in to Access Finland within a month, your account was deleted.</p>
                    "
            },
            _ => throw new ArgumentException($"Email template {template} not found"),
        };
    }

    private record EmailTemplate
    {
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }

    public enum NotificationTemplate
    {
        AccountToBeDeletedFromInactivity,
        AccountDeletedFromInactivity
    }
}
