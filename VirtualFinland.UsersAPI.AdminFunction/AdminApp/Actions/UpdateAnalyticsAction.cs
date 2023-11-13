using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Sets up the database user for the application. 
/// The intented use is to allow the pulumi deployment process to create and manage the application level database user.
/// In live environments the script is called during a deployment (finishing phases) from a cloud function residing in the same virtual private cloud (VPC) as the database.
/// </summary>
public class UpdateAnalyticsAction : IAdminAppAction
{
    private readonly AnalyticsConfig.CloudWatchSettings _cloudWatchSettings;
    private readonly UsersDbContext _dataContext;
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly ILogger<UpdateAnalyticsAction> _logger;

    public UpdateAnalyticsAction(AnalyticsConfig settings, UsersDbContext dataContext, IAmazonCloudWatch cloudWatchClient, ILogger<UpdateAnalyticsAction> logger)
    {
        _cloudWatchSettings = settings.CloudWatch;
        _dataContext = dataContext;
        _cloudWatchClient = cloudWatchClient;
        _logger = logger;
    }

    public async Task Execute(string? _)
    {
        if (!_cloudWatchSettings.IsEnabled)
        {
            _logger.LogInformation("CloudWatch analytics is disabled");
            return;
        }

        // Gather statistics
        var metricData = new List<MetricDatum>();

        var personsCount = await _dataContext.Persons.CountAsync();
        _logger.LogInformation("PersonsCount: {PersonsCount}", personsCount);
        metricData.Add(new()
        {
            MetricName = "PersonsCount",
            Value = personsCount,
            Unit = StandardUnit.None,
            TimestampUtc = DateTime.UtcNow
        });

        var personsCountByIssuers = await _dataContext.ExternalIdentities.GroupBy(x => x.Issuer).Select(x => new { Issuer = x.Key, Count = x.Count() }).ToListAsync();
        foreach (var personsCountByIssuer in personsCountByIssuers)
        {
            if (string.IsNullOrEmpty(personsCountByIssuer.Issuer))
            {
                continue;
            }

            _logger.LogInformation("PersonsCountByIssuer: {Issuer} {Count}", personsCountByIssuer.Issuer, personsCountByIssuer.Count);
            metricData.Add(new()
            {
                MetricName = "PersonsCountByIssuer",
                Value = personsCountByIssuer.Count,
                Unit = StandardUnit.None,
                TimestampUtc = DateTime.UtcNow,
                Dimensions = new List<Dimension>()
                {
                    new()
                    {
                        Name = "Issuer",
                        Value = personsCountByIssuer.Issuer
                    }
                }
            });
        }

        // Retrieves the amount of persons by audience
        // Note: as of the column data is stored as a comma separated string and modelled as List<string> in the code (the model-first approach),
        // composing this query with fluent syntax proved to be rather difficult. Maybe revisit this with newer version of EF Core.
        var rawQuery = @"
            SELECT COUNT(*) AS Amount, regexp_split_to_table(""Audiences"", ',') AS Audience  
            FROM ""ExternalIdentities""
            GROUP BY Audience;
        ";
        var personsCountByAudiences = _dataContext.PersonsByAudiencesResults.FromSqlRaw(rawQuery).ToList();

        foreach (var personsCountByAudience in personsCountByAudiences)
        {
            if (string.IsNullOrEmpty(personsCountByAudience.Audience))
            {
                continue;
            }

            _logger.LogInformation("PersonsCountByAudience: {Audience} {Amount}", personsCountByAudience.Audience, personsCountByAudience.Amount);
            metricData.Add(new()
            {
                MetricName = "PersonsCountByAudience",
                Value = personsCountByAudience.Amount,
                Unit = StandardUnit.None,
                TimestampUtc = DateTime.UtcNow,
                Dimensions = new List<Dimension>()
                {
                    new()
                    {
                        Name = "Audience",
                        Value = personsCountByAudience.Audience
                    }
                }
            });
        }

        // Update analytics
        await _cloudWatchClient.PutMetricDataAsync(new PutMetricDataRequest()
        {
            MetricData = metricData,
            Namespace = _cloudWatchSettings.Namespace
        });

    }

    public record AnalyticsEvent(string Username);
}