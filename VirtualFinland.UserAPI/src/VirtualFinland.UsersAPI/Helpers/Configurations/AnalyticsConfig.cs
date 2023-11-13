namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class AnalyticsConfig
{
    public AnalyticsConfig(IConfiguration configuration)
    {
        CloudWatch = configuration.GetSection("Analytics:CloudWatch").Get<CloudWatchSettings>();
        Sqs = configuration.GetSection("Analytics:SQS").Get<SqsSettings>();
    }
    public AnalyticsConfig(CloudWatchSettings cloudWatch, SqsSettings sqs)
    {
        CloudWatch = cloudWatch;
        Sqs = sqs;
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