using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsService<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;

    public AnalyticsService(ILogger<T> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Log an audit log event
    /// </summary>
    /// <param name="auditEvent">The audit log event</param>
    /// <param name="eventContextInfo">The event context info, TODO: enumify this params</param>
    /// <param name="requestAuthenticatedUser">The authenticated user</param>
    /// <returns></returns>
    public void LogAuditLogEvent(AuditLogEvent auditEvent, string eventContextInfo, RequestAuthenticatedUser requestAuthenticatedUser)
    {
        _logger.LogInformation("AuditLog: {auditEvent} ({eventContextInfo}) on {user}", auditEvent.ToString().ToUpper(), eventContextInfo, requestAuthenticatedUser);
        if (eventContextInfo == "Person" && (auditEvent == AuditLogEvent.Create || auditEvent == AuditLogEvent.Delete))
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
