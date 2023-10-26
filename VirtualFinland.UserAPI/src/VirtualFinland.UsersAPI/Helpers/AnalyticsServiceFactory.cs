
using Amazon.CloudWatch;
using Amazon.SQS;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public class AnalyticsServiceFactory
{
    private readonly IOptions<AnalyticsConfig> _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly IAmazonSQS _sqsClient;

    public AnalyticsServiceFactory(IOptions<AnalyticsConfig> options, ILoggerFactory loggerFactory, IAmazonCloudWatch cloudWatchClient, IAmazonSQS sqsClient)
    {
        _options = options;
        _loggerFactory = loggerFactory;
        _cloudWatchClient = cloudWatchClient;
        _sqsClient = sqsClient;
    }

    public virtual AnalyticsService<T> CreateAnalyticsService<T>()
    {
        return new AnalyticsService<T>(
            _options,
            _loggerFactory.CreateLogger<T>(),
            _cloudWatchClient,
            _sqsClient
        );
    }
}
