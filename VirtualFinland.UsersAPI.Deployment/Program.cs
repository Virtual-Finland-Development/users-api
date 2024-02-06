using System.Collections.Generic;
using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using VirtualFinland.UsersAPI.Deployment.Features;

return await Deployment.RunAsync(async () =>
{
    var config = new Config();
    var organization = Deployment.Instance.OrganizationName;
    var environment = Deployment.Instance.StackName;
    var projectName = Deployment.Instance.ProjectName;

    InputMap<string> tags = new()
        {
            {
                "vfd:stack", Deployment.Instance.StackName
            },
            {
                "vfd:project", Deployment.Instance.ProjectName
            }
        };

    var stackSetup = new StackSetup()
    {
        Organization = organization,
        ProjectName = projectName,
        Environment = environment,
        Region = new Config("aws").Require("region"),
        Tags = tags,
    };

    var isInitialDeployment = await stackSetup.IsInitialDeployment();

    var cloudwatch = new CloudWatch(stackSetup);
    var vpcSetup = new VpcSetup(stackSetup);
    var database = new PostgresDatabase(config, stackSetup, vpcSetup, cloudwatch);
    var dbConnectionStringSecret = new SecretsManager(stackSetup, "dbConnectionStringSecret", database.DatabaseConnectionString);
    var dbAdminConnectionStringSecret = new SecretsManager(stackSetup, "dbAdminConnectionStringSecret", database.DatabaseAdminConnectionString);
    var auditLogSubscriptionFunction = new AuditLogSubscription(config, stackSetup, database, cloudwatch);
    var redisCache = new RedisElastiCache(stackSetup, vpcSetup);

    var adminFunctionSqses = SqsQueue.CreateSqsQueueForAdminCommands(stackSetup);

    // The API
    var usersApiFunction = new UsersApiLambdaFunction(config, stackSetup, vpcSetup, dbConnectionStringSecret, redisCache, cloudwatch, adminFunctionSqses, database);
    usersApiFunction.SetupErrorAlerting(stackSetup);
    var apiEndpoint = new LambdaFunctionUrl(stackSetup, usersApiFunction);

    // Admin function for management tasks, scheduled events and triggers
    var adminFunction = new AdminFunction(config, stackSetup, vpcSetup, dbAdminConnectionStringSecret, adminFunctionSqses, database, redisCache);

    // Admin function schedulers and triggers
    adminFunction.CreateSchedulersAndTriggers(stackSetup, adminFunctionSqses);

    if (isInitialDeployment)
    {
        // Initialize database
        database.InvokeInitialDatabaseSetupFunction(stackSetup, adminFunction.LambdaFunction);
    }
    else
    {
        // Ensure database user 
        database.InvokeDatabaseUserSetupFunction(stackSetup, adminFunction.LambdaFunction);
        // Ensure database audit log triggers
        database.InvokeDatabaseAuditLogTriggersSetupFunction(stackSetup, adminFunction.LambdaFunction);
    }

    // Outputs
    return new Dictionary<string, object?> {
        { "ApplicationUrl", apiEndpoint.ApplicationUrl },
        { "LambdaId", usersApiFunction.LambdaFunctionId },
        { "DBIdentifier", database.DBIdentifier },
        { "DBClusterIdentifier", database.DBClusterIdentifier },
        { "AdminFunctionArn", adminFunction.LambdaFunction.Arn },
        { "AuditLogSubscriptionFunctionArn", auditLogSubscriptionFunction.LambdaFunctionArn },
        { "ElastiCacheClusterId", redisCache.ClusterId },
    };
});