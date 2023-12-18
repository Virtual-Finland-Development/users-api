namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class NotificationsConfig
{
    public readonly EmailConfigValues Email;

    public NotificationsConfig(IConfiguration configuration)
    {
        Email = configuration.GetSection("Services:Notifications:Email").Get<EmailConfigValues>();
    }

    public record EmailConfigValues
    {
        public bool IsEnabled { get; init; }
        public string SenderAddress { get; init; } = string.Empty;
    }
}