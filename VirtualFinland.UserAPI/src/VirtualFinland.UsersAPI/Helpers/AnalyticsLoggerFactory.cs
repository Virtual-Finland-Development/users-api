
using VirtualFinland.UserAPI.Helpers.Services;

namespace VirtualFinland.UserAPI.Helpers;

public class AnalyticsLoggerFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly AnalyticsService _analyticsService;

    public AnalyticsLoggerFactory(ILoggerFactory loggerFactory, AnalyticsService analyticsService)
    {
        _loggerFactory = loggerFactory;
        _analyticsService = analyticsService;
    }

    public virtual AnalyticsLogger<T> CreateAnalyticsLogger<T>()
    {
        return new AnalyticsLogger<T>(
            _loggerFactory.CreateLogger<T>(),
            _analyticsService
        );
    }
}
