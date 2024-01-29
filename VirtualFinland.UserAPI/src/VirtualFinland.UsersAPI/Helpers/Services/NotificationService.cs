using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class NotificationService
{
    private readonly NotificationsConfig _config;
    private readonly ILogger<NotificationService> _logger;
    private readonly EmailTemplates _emailTemplates;
    public NotificationService(NotificationsConfig notificationsConfig, EmailTemplates emailTemplates, ILogger<NotificationService> logger)
    {
        _config = notificationsConfig;
        _emailTemplates = emailTemplates;
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

        var templateData = _emailTemplates.GetEmailTemplateForPersonEmail(template, person);

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
                        Data = templateData.HtmlBody
                    },
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = templateData.TextBody
                    }
                }
            },
        };

        await client.SendEmailAsync(sendRequest); // @TODO: Should be send as queued task that is retried
    }

    public enum NotificationTemplate
    {
        AccountToBeDeletedFromInactivity,
        AccountDeletedFromInactivity
    }
}
