using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Sets up the database user for the application. 
/// The intented use is to allow the pulumi deployment process to create and manage the application level database user.
/// In live environments the script is called during a deployment (finishing phases) from a cloud function residing in the same virtual private cloud (VPC) as the database.
/// </summary>
public class UpdateAnalyticsAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly ILogger<UpdateAnalyticsAction> _logger;

    public UpdateAnalyticsAction(UsersDbContext dataContext, IAmazonCloudWatch cloudWatchClient, ILogger<UpdateAnalyticsAction> logger)
    {
        _dataContext = dataContext;
        _cloudWatchClient = cloudWatchClient;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        // Gather statistics
        var personsCount = await _dataContext.Persons.CountAsync();

        _logger.LogInformation("PersonsCount: {PersonsCount}", personsCount);

        // Update analytics
        await _cloudWatchClient.PutMetricDataAsync(new PutMetricDataRequest()
        {
            MetricData = new List<MetricDatum>()
            {
                new()
                {
                    MetricName = "PersonsCount",
                    Value = personsCount,
                    Unit = StandardUnit.None,
                    TimestampUtc = DateTime.UtcNow
                }
            },
            Namespace = Constants.Analytics.Namespace
        });

    }

    public record AnalyticsEvent(string Username);
}