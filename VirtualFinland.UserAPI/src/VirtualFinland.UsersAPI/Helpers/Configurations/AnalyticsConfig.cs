namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class AnalyticsConfig
{
    public AnalyticsConfig(IConfiguration configuration)
    {
        CloudWatch = configuration.GetValue<CloudWatchSettings>("Analytics:CloudWatch");
    }

    public AnalyticsConfig()
    {
    }

    public CloudWatchSettings CloudWatch { get; set; } = new();

    public record CloudWatchSettings
    {
        public bool IsEnabled { get; set; }
        public string Namespace { get; set; } = null!;
    }
}