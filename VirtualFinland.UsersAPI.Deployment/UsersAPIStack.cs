using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using VirtualFinland.UsersAPI.Deployment.Features;

namespace VirtualFinland.UsersAPI.Deployment;

public class UsersApiStack : Stack
{
    public UsersApiStack()
    {
        var config = new Config();
        bool isProductionEnvironment = IsProductionEnvironment();
        var organization = Pulumi.Deployment.Instance.OrganizationName;
        var environment = Pulumi.Deployment.Instance.StackName;
        var projectName = Pulumi.Deployment.Instance.ProjectName;

        InputMap<string> tags = new()
        {
            {
                "vfd:stack", Pulumi.Deployment.Instance.StackName
            },
            {
                "vfd:project", Pulumi.Deployment.Instance.ProjectName
            }
        };

        var stackSetup = new StackSetup()
        {
            Organization = organization,
            ProjectName = projectName,
            Environment = environment,
            IsProductionEnvironment = isProductionEnvironment,
            Tags = tags,
        };

        var cloudwatch = new CloudWatch(stackSetup);
        var vpcSetup = new VpcSetup(stackSetup);
        var database = new PostgresDatabase(config, stackSetup, vpcSetup, cloudwatch);
        var dbConnectionStringSecret = new SecretsManager(stackSetup, "dbConnectionStringSecret", database.DatabaseConnectionString);
        var dbAdminConnectionStringSecret = new SecretsManager(stackSetup, "dbAdminConnectionStringSecret", database.DatabaseAdminConnectionString);
        var auditLogSubscriptionFunction = new AuditLogSubscription(config, stackSetup, database, cloudwatch);
        var redisCache = new RedisElastiCache(stackSetup, vpcSetup);

        var analyticsSqS = SqsQueue.CreateSqsQueueForAnalyticsCommand(stackSetup);
        var usersApiFunction = new UsersApiLambdaFunction(config, stackSetup, vpcSetup, dbConnectionStringSecret, redisCache, cloudwatch, analyticsSqS);
        var apiProvider = new LambdaFunctionUrl(stackSetup, usersApiFunction);

        ApplicationUrl = apiProvider.ApplicationUrl;
        LambdaId = usersApiFunction.LambdaFunctionId;
        DBIdentifier = database.DBIdentifier;
        AuditLogSubscriptionFunctionArn = auditLogSubscriptionFunction.LambdaFunctionArn;

        var adminFunction = new AdminFunction(config, stackSetup, vpcSetup, dbAdminConnectionStringSecret, analyticsSqS);
        AdminFunctionArn = adminFunction.LambdaFunction.Arn;

        // Ensure database user 
        database.InvokeInitialDatabaseUserSetupFunction(stackSetup, adminFunction.LambdaFunction);
        // Ensure database audit log triggers
        database.InvokeInitialDatabaseAuditLogTriggersSetupFunction(stackSetup, adminFunction.LambdaFunction);
    }

    private static bool IsProductionEnvironment()
    {
        var stackName = Pulumi.Deployment.Instance.StackName;
        return stackName switch
        {
            // Cheers: https://stackoverflow.com/a/65642709
            var value when value == Environments.MvpProduction => true,
            var value when value == Environments.MvpStaging => true,
            _ => false,
        };
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
    [Output] public Output<string>? DBIdentifier { get; set; }
    [Output] public Output<string>? AdminFunctionArn { get; set; }
    [Output] public Output<string>? AuditLogSubscriptionFunctionArn { get; set; }
}
