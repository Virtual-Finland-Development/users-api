namespace VirtualFinland.UserAPI.Helpers.Configurations;

using VirtualFinland.UserAPI.Models.App;

public class AnalyticsConfig
{
    public AnalyticsConfig(IConfiguration configuration)
    {
        CloudWatch = configuration.GetSection("Analytics:CloudWatch").Get<CloudWatchSettings>();
        Sqs = configuration.GetSection("Analytics:SQS").Get<AnalyticsSqsSettings>();
    }
    public AnalyticsConfig(CloudWatchSettings cloudWatch, AnalyticsSqsSettings sqs)
    {
        CloudWatch = cloudWatch;
        Sqs = sqs;
    }

    public CloudWatchSettings CloudWatch { get; set; } = new();
    public AnalyticsSqsSettings Sqs { get; set; } = new();

    public record AnalyticsSqsSettings
    {
        public bool IsEnabled { get; init; }
        public string QueueUrl { get; init; } = null!;
    }
}