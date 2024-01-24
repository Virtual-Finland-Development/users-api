using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using static VirtualFinland.UserAPI.Helpers.Services.NotificationService;

namespace VirtualFinland.UserAPI.Helpers;

public class EmailTemplates
{
  private readonly string _siteUrl;
  public EmailTemplates(NotificationsConfig notificationsConfig)
  {
    _siteUrl = notificationsConfig.Email.SiteUrl;
  }

  public EmailTemplate GetEmailTemplateForPersonEmail(NotificationTemplate template, Person person)
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
                        <p><a href=""{_siteUrl}"">{_siteUrl}</a></p>
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
                        <p><a href=""{_siteUrl}"">{_siteUrl}</a></p>
                    "),
        TextBody = "Your Access Finland account was deleted from inactivity!"
      },
      _ => throw new ArgumentException($"Email template {template} not found"),
    };
  }

  private static string WrapEmailHtmlContentWithCoreTemplate(string title, string htmlBodyContent)
  {
    return @$"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
      body {{
        font-family: Arial, Helvetica, sans-serif;
        font-size: 14px;
      }}
    </style>
  </head>
  <body>
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"" width=""100%"" style=""max-width: 600px;"">
      <tr>
        <td>
          {htmlBodyContent}
          <hr />
          <p>This message cannot be replied to.</p>
        </td>
      </tr>
    </table>
  </body>
</html>";
  }

  public record EmailTemplate
  {
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string TextBody { get; init; } = string.Empty;
  }
}
