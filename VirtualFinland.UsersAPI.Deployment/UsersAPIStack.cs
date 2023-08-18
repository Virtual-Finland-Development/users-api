using Pulumi;
using Pulumi.Aws.Ec2;
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
            ProjectName = projectName,
            Environment = environment,
            IsProductionEnvironment = isProductionEnvironment,
            Tags = tags,
        };

        var vpcSetup = new VpcSetup(stackSetup);
        var database = new PostgresDatabase(config, stackSetup, vpcSetup);
        var secretManagerSecret = new SecretsManager(stackSetup, "dbConnectionStringSecret", database.DatabaseConnectionString);

        var lambdaFunctionConfigs = new LambdaFunctionUrl(config, stackSetup, vpcSetup, secretManagerSecret);
        ApplicationUrl = lambdaFunctionConfigs.ApplicationUrl;
        LambdaId = lambdaFunctionConfigs.LambdaFunctionId;
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
            var value when value == Environments.MvpProduction.ToString().ToLowerInvariant() => true,
            var value when value == Environments.MvpStaging.ToString().ToLowerInvariant() => true,
            _ => false,
        };
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
    [Output] public Output<string>? DBIdentifier { get; set; }
    [Output] public Output<string>? DatabaseMigratorLambdaArn { get; set; }
}
