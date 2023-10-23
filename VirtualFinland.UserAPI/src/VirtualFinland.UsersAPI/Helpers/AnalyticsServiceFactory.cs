
using Amazon.CloudWatch;
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public class AnalyticsServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IAmazonCloudWatch _cloudWatchClient;

    public AnalyticsServiceFactory(ILoggerFactory loggerFactory, IAmazonCloudWatch cloudWatchClient)
    {
        _loggerFactory = loggerFactory;
        _cloudWatchClient = cloudWatchClient;
    }

    public virtual AnalyticsService<T> CreateAnalyticsService<T>()
    {
        return new AnalyticsService<T>(
            _loggerFactory.CreateLogger<T>(),
            _cloudWatchClient
        );
    }
}
