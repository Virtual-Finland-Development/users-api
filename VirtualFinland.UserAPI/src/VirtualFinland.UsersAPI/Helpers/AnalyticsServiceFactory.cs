
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public class AnalyticsServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public AnalyticsServiceFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public virtual AnalyticsService<T> CreateAnalyticsService<T>()
    {
        return new AnalyticsService<T>(
            _loggerFactory.CreateLogger<T>()
        );
    }
}
