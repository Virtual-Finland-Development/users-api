using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsService<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly Type _handlerType = typeof(T);

    public AnalyticsService(ILogger<T> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Log an audit log event
    /// </summary>
    /// <param name="auditEvent">The audit log event</param>
    /// <param name="requestAuthenticatedUser">The authenticated user</param>
    /// <param name="eventContextName">Context name for the log</param>
    /// <returns></returns>
    public void LogAuditLogEvent(AuditLogEvent auditEvent, RequestAuthenticatedUser requestAuthenticatedUser, string? eventContextName = null)
    {
        if (eventContextName == null) eventContextName = _handlerType.ToString();

        _logger.LogInformation("AuditLog: {auditEvent} ({eventContextName}) on {userInfo}",
            auditEvent.ToString().ToUpper(),
            eventContextName,
            requestAuthenticatedUser
        );
        if (eventContextName == "Person" && (auditEvent == AuditLogEvent.Create || auditEvent == AuditLogEvent.Delete))
        {
            // Fire an analytics update event..
        }
    }

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
}
