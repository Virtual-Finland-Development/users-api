using System.Text.Json;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Security.Models;

namespace VirtualFinland.UserAPI.Helpers.Services;

public class AnalyticsService
{
    private readonly AnalyticsConfig _analyticsConfig;
    private readonly IAmazonCloudWatch _cloudwatchClient;
    private readonly IAmazonSQS _sqsClient;

    public AnalyticsService(AnalyticsConfig options, IAmazonCloudWatch cloudwatchClient, IAmazonSQS sqsClient)
    {
        _analyticsConfig = options;
        _cloudwatchClient = cloudwatchClient;
        _sqsClient = sqsClient;
    }


    /// <summary>
    /// Publish request as an analytics event
    /// </summary>
    /// <param name="requestAuthenticatedUser">The authenticated user</param>
    /// <param name="eventContextName">Context name for the log, defaults to the parsed form of the generic type T</param>
    /// <returns></returns>
    public async Task HandleAuthenticatedRequest(RequestAuthenticatedUser requestAuthenticatedUser, string eventContext)
    {
        if (!_analyticsConfig.CloudWatch.IsEnabled)
        {
            return;
        }

        // Publish AWS Cloudwatch custom metrics for the request
        await _cloudwatchClient.PutMetricDataAsync(new PutMetricDataRequest()
        {
            MetricData = new List<MetricDatum>()
            {
                new()
                {
                    MetricName = "RequestsTotal",
                    Value = 1,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow
                },
                new()
                {
                    MetricName = "RequestsPerContext",
                    Value = 1,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow,
                    Dimensions = new List<Dimension>()
                    {
                        new()
                        {
                            Name = "Context",
                            Value = eventContext
                        }
                    }
                },
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
                    MetricName = "RequestsTotalPerIssuer",
                    Value = 1,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow,
                    Dimensions = new List<Dimension>()
                    {
                        new()
                        {
                            Name = "Issuer",
                            Value = requestAuthenticatedUser.Issuer
                        }
                    }
                },
            },
            Namespace = _analyticsConfig.CloudWatch.Namespace
        });
    }

    /// <summary>
    /// Handle special analytics events
    /// </summary>
    public async Task HandleSpecialAnalyticEvents(string eventContextName)
    {
        if (!_analyticsConfig.Sqs.IsEnabled)
        {
            return;
        }

        //
        // Analytics events for the Persons table inserts and deletes
        //
        var personsTableCountChangedEventContextNames = new[] {
            "AuthenticationService::AuthenticateAndGetOrRegisterAndGetPerson",
            "DeleteUser",
        };
        if (personsTableCountChangedEventContextNames.Contains(eventContextName))
        {
            // Publish an SQS message to the analytics updater lambda
            await _sqsClient.SendMessageAsync(new SendMessageRequest()
            {
                QueueUrl = _analyticsConfig.Sqs.QueueUrl,
                MessageGroupId = "PersonsTableCountChanged",
                MessageBody = JsonSerializer.Serialize(new
                {
                    Action = "UpdateAnalytics"
                }),
            });
        }
    }
}
