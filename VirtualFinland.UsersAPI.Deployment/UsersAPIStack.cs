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

        InputMap<string> tags = new InputMap<string>()
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
        var dbConfigs = new PostgresDatabase(config, stackSetup, vpcSetup);
        var secretManagerSecret = new SecretsManager(config, stackSetup, dbConfigs);

        var usersApiFunction = new UsersApiLambdaFunction(config, stackSetup, vpcSetup, secretManagerSecret);
        var apiProvider = new LambdaFunctionUrl(stackSetup, usersApiFunction);

        ApplicationUrl = apiProvider.ApplicationUrl;
        LambdaId = usersApiFunction.LambdaFunctionId;
        DBIdentifier = dbConfigs.DBIdentifier;

        var databaseMigratorLambda = new DatabaseMigratorLambda(config, stackSetup, vpcSetup, secretManagerSecret);
        DatabaseMigratorLambdaArn = databaseMigratorLambda.LambdaFunctionArn;
    }

    private bool IsProductionEnvironment()
    {
        var stackName = Pulumi.Deployment.Instance.StackName;
        switch (stackName)
        {
            // Cheers: https://stackoverflow.com/a/65642709
            case var value when value == Environments.Production.ToString().ToLowerInvariant():
                return true;
            default:
                return false;
        }
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
    [Output] public Output<string>? DBIdentifier { get; set; }
    [Output] public Output<string>? DatabaseMigratorLambdaArn { get; set; }
}
