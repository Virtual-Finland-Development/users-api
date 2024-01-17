namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class NotificationsConfig
{
    public readonly EmailConfigValues Email;

    public NotificationsConfig(IConfiguration configuration)
    {
        Email = configuration.GetSection("Notifications:Email").Get<EmailConfigValues>();
    }

    public NotificationsConfig(EmailConfigValues email)
    {
        Email = email;
    }

    public record EmailConfigValues
    {
        public bool IsEnabled { get; init; }
        public string FromAddress { get; init; } = string.Empty;
        public string SiteUrl { get; init; } = string.Empty;
    }
}