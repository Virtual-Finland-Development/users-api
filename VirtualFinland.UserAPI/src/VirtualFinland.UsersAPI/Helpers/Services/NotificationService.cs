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

        await client.SendEmailAsync(sendRequest);
    }

    private EmailTemplate GetEmailTemplateData(Person person, NotificationTemplate template)
    {
        var personFirstName = person.GivenName ?? person.Email;
        return template switch
        {
            NotificationTemplate.AccountToBeDeletedFromInactivity => new EmailTemplate
            {
                Subject = "Your Access Finland account will be deleted from inactivity!",
                HtmlBody = WrapEmailHtmlContentWithCoreTemplate("Your Access Finland account will be deleted from inactivity!", @$"
                        <h1>Your Access Finland account will be deleted from inactivity!</h1>
                        <p>Hi {personFirstName}, you have not been active in Access Finland for a long time.</p>
                        <p>Unless you log in to Access Finland within a month, your account will be automatically deleted.</p>
                        <p>If you want to keep your account, please log in to Access Finland here:</p>
                        <p><a href=""{_config.Email.SiteUrl}"">{_config.Email.SiteUrl}</a></p>
                    "),
                TextBody = "Your Access Finland account will be deleted from inactivity in 30 days!"
            },
            NotificationTemplate.AccountDeletedFromInactivity => new EmailTemplate
            {
                Subject = "Your Access Finland account was deleted from inactivity!",
                HtmlBody = WrapEmailHtmlContentWithCoreTemplate("Your Access Finland account was deleted from inactivity!", @$"
                        <h1>Your Access Finland account was deleted from inactivity!</h1>
                        <p>Hello {personFirstName}, a month ago we sent you an email about your Access Finland account being deleted from inactivity.</p>
                        <p>Since you did not log in to Access Finland within a month, your account was deleted.</p>
                        <p>If you want to continue using Access Finland service, please create a new account here:</p>
                        <p><a href=""{_config.Email.SiteUrl}"">{_config.Email.SiteUrl}</a></p>
                    "),
                TextBody = "Your Access Finland account was deleted from inactivity!"
            },
            _ => throw new ArgumentException($"Email template {template} not found"),
        };
    }

    private static string WrapEmailHtmlContentWithCoreTemplate(string title, string body)
    {
        return @$"
            <html>
                <head>
                    <title>{title}</title>
                    <style>
                        body: {{
                            font-family: Arial, Helvetica, sans-serif;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    {body}
                    <hr />
                    <p>This message cannot be replied to.</p>
                </body>
            </html>
        ";
    }

    private record EmailTemplate
    {
        public string Subject { get; init; } = string.Empty;
        public string HtmlBody { get; init; } = string.Empty;
        public string TextBody { get; init; } = string.Empty;
    }

    public enum NotificationTemplate
    {
        AccountToBeDeletedFromInactivity,
        AccountDeletedFromInactivity
    }
}
