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

        var vpcSetup = new VpcSetup(stackSetup);
        var database = new PostgresDatabase(config, stackSetup, vpcSetup);
        var secretManagerSecret = new SecretsManager(stackSetup, "dbConnectionStringSecret", database.DatabaseConnectionString);
        var redisCache = new RedisElastiCache(stackSetup, vpcSetup);

        var usersApiFunction = new UsersApiLambdaFunction(config, stackSetup, vpcSetup, secretManagerSecret, redisCache);
        var apiProvider = new LambdaFunctionUrl(stackSetup, usersApiFunction);

        ApplicationUrl = apiProvider.ApplicationUrl;
        LambdaId = usersApiFunction.LambdaFunctionId;
        DBIdentifier = database.DBIdentifier;

        var databaseMigratorLambda = new DatabaseMigratorLambda(config, stackSetup, vpcSetup, secretManagerSecret);
        DatabaseMigratorLambdaArn = databaseMigratorLambda.LambdaFunctionArn;
    }

    private bool IsProductionEnvironment()
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
    [Output] public Output<string>? DatabaseMigratorLambdaArn { get; set; }
}
