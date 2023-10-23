using Amazon.CloudWatch;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsService<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly Type _handlerType = typeof(T);
    private readonly IAmazonCloudWatch _cloudwatchClient;

    public AnalyticsService(ILogger<T> logger, IAmazonCloudWatch cloudwatchClient)
    {
        _logger = logger;
        _cloudwatchClient = cloudwatchClient;
    }

    /// <summary>
    /// Log an audit log event
    /// </summary>
    /// <param name="auditEvent">The audit log event</param>
    /// <param name="requestAuthenticatedUser">The authenticated user</param>
    /// <param name="eventContextName">Context name for the log, defaults to the parsed form of the generic type T</param>
    /// <returns></returns>
    public void LogAuditLogEvent(AuditLogEvent auditEvent, RequestAuthenticatedUser requestAuthenticatedUser, string? eventContextName = null)
    {
        eventContextName = ParseHandlerTypeContextName(eventContextName);
        var auditEventFormatted = auditEvent.ToString().ToUpper();

        _logger.LogInformation("AuditLog: {auditEventFormatted}-event on {userInfo} from {eventContextName}",
            auditEventFormatted,
            requestAuthenticatedUser,
            eventContextName
        );

        PutCloudWatchCustomMetrics(requestAuthenticatedUser);

        //
        // Analytics events for the Persons table inserts and deletes
        //
        var personsTableCountChangedEventContextNames = new[] {
            "AuthenticationService::AuthenticateAndGetOrRegisterAndGetPerson",
            "DeleteUser",
        };
        if (personsTableCountChangedEventContextNames.Contains(eventContextName))
        {
            if (auditEvent != AuditLogEvent.Create && auditEvent != AuditLogEvent.Delete)
            {
                throw new Exception($"AuditLogEvent {auditEventFormatted} is not supported for {eventContextName}");
            }

            _logger.LogInformation(">>> FIRE ANALYTICS UPDATER CALL HERE <<<");
        }
    }

    private void PutCloudWatchCustomMetrics(RequestAuthenticatedUser requestAuthenticatedUser)
    {
        _cloudwatchClient.PutMetricDataAsync(new Amazon.CloudWatch.Model.PutMetricDataRequest()
        {
            MetricData = new List<Amazon.CloudWatch.Model.MetricDatum>() {
                new() {
                    MetricName = "Audiences",
                    Dimensions = new List<Amazon.CloudWatch.Model.Dimension>() {
                        new() {
                            Name = "Audience",
                            Value = requestAuthenticatedUser.Audience
                        },
                    },
                    Unit = StandardUnit.Count,
                    Value = 1
                }
            },
            Namespace = "VirtualFinland.UserAPI"
        });
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
