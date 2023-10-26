namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class AnalyticsConfig
{
    public AnalyticsConfig(IConfiguration configuration)
    {
        CloudWatch = configuration.GetValue<CloudWatchSettings>("Analytics:CloudWatch");
        Sqs = configuration.GetValue<SqsSettings>("Analytics:Sqs");
    }

    public AnalyticsConfig()
    {
    }

    public CloudWatchSettings CloudWatch { get; set; } = new();
    public SqsSettings Sqs { get; set; } = new();

    public record CloudWatchSettings
    {
        public bool IsEnabled { get; set; }
        public string Namespace { get; set; } = null!;
    }

    public record SqsSettings
    {
        public bool IsEnabled { get; set; }
        public string QueueUrl { get; set; } = null!;
    }
}