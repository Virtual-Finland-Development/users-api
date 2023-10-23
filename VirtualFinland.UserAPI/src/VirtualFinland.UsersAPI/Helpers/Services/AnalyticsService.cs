using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Options;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsService<T> : ILogger<T>
{
    private readonly AnalyticsConfig.CloudWatchSettings _cloudWatchSettings;
    private readonly ILogger<T> _logger;
    private readonly Type _handlerType = typeof(T);
    private readonly IAmazonCloudWatch _cloudwatchClient;

    public AnalyticsService(IOptions<AnalyticsConfig> options, ILogger<T> logger, IAmazonCloudWatch cloudwatchClient)
    {
        _cloudWatchSettings = options.Value.CloudWatch;
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
        var eventContext = ParseHandlerTypeContextName(eventContextName);

        _logger.LogInformation("AuditLog: {auditEvent}-event on {userInfo} from {eventContext}",
            auditEvent,
            requestAuthenticatedUser,
            eventContext
        );

        PutUpdateToRequestMetrics(requestAuthenticatedUser, eventContext);
    }

    public void PutUpdateToRequestMetrics(RequestAuthenticatedUser requestAuthenticatedUser, string? eventContextName = null)
    {
        if (!_cloudWatchSettings.IsEnabled)
        {
            return;
        }

        var eventContext = ParseHandlerTypeContextName(eventContextName);
        PutCloudWatchCustomMetrics(requestAuthenticatedUser, eventContext);
        EngageInSpecialAnalyticEvents(eventContext);
    }

    private void PutCloudWatchCustomMetrics(RequestAuthenticatedUser requestAuthenticatedUser, string? eventContextName = null)
    {
        // Push an AWS Cloudwatch custom metric that counts the number of requests per audience
        _cloudwatchClient.PutMetricDataAsync(new PutMetricDataRequest()
        {
            MetricData = new List<MetricDatum>()
            {
                new()
                {
                    MetricName = "RequestsTotalPerAudience",
                    Value = 1,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow,
                    Dimensions = new List<Dimension>()
                    {
                        new()
                        {
                            Name = "Audience",
                            Value = requestAuthenticatedUser.Audience
                        }
                    }
                },
                new()
                {
                    MetricName = "RequestsPerAudience",
                    Value = 1,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow,
                    Dimensions = new List<Dimension>()
                    {
                        new()
                        {
                            Name = "Audience",
                            Value = requestAuthenticatedUser.Audience
                        },
                        new()
                        {
                            Name = "Context",
                            Value = eventContextName
                        }
                    }
                }
            },
            Namespace = _cloudWatchSettings.Namespace
        });
    }

    private void EngageInSpecialAnalyticEvents(string eventContextName)
    {
        //
        // Analytics events for the Persons table inserts and deletes
        //
        var personsTableCountChangedEventContextNames = new[] {
            "AuthenticationService::AuthenticateAndGetOrRegisterAndGetPerson",
            "DeleteUser",
        };
        if (personsTableCountChangedEventContextNames.Contains(eventContextName))
        {
            _logger.LogInformation(">>> FIRE ANALYTICS UPDATER CALL HERE <<<");
        }
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
