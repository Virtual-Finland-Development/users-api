using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly AnalyticsService _analyticsService;
    private readonly Type _handlerType = typeof(T);

    public AnalyticsLogger(ILogger<T> logger, AnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Log an audit log event
    /// </summary>
    /// <param name="auditEvent">The audit log event</param>
    /// <param name="requestAuthenticatedUser">The authenticated user</param>
    /// <param name="eventContextName">Context name for the log, defaults to the parsed form of the generic type T</param>
    /// <returns></returns>
    public async Task LogAuditLogEvent(AuditLogEvent auditEvent, RequestAuthenticatedUser requestAuthenticatedUser, string? eventContextName = null)
    {
        var eventContext = ParseHandlerTypeContextName(eventContextName);

        _logger.LogInformation("AuditLog: {auditEvent}-event on {userInfo} from {eventContext}",
            auditEvent,
            requestAuthenticatedUser,
            eventContext
        );

        await _analyticsService.HandleSpecialAnalyticEvents(eventContext);
    }


    /// <summary>
    /// Parse the handler type context name from the handler type
    /// eg. VirtualFinland.UserAPI.Activities.Productizer.Operations.BasicInformation.GetPersonBasicInformation+Handler -> GetPersonBasicInformation
    /// </summary>
    /// <returns></returns>
    private string ParseHandlerTypeContextName(string? eventContextName = null)
    {
        if (eventContextName is not null) return eventContextName;

        var handlerType = _handlerType.ToString();
        var handlerTypeParts = handlerType.Split('.');
        var handlerTypeContextName = handlerTypeParts[^1];
        if (handlerTypeContextName.Contains('+'))
        {
            handlerTypeContextName = handlerTypeContextName.Split('+')[0];
        }
        return handlerTypeContextName;
    }


    //---> ILogger<T> implementations
    public IDisposable BeginScope<TState>(TState state)
    {
        return _logger.BeginScope(state);
    }
    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
    //<--- ILogger<T> implementations
}
