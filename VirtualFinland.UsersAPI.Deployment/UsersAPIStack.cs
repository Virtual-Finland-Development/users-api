using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        var environment = Pulumi.Deployment.Instance.StackName;
        var projectName = Pulumi.Deployment.Instance.ProjectName;

        var infraStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/{config.Require("infraStackReferenceName")}/{environment}");
        var infraStackReferencePrivateSubnetIds = infraStackReference.RequireOutput("PrivateSubnetIds");
        var infraStackReferencePrivateSubnetIdsAsList = infraStackReferencePrivateSubnetIds.Apply(o => ((ImmutableArray<object>)(o ?? new ImmutableArray<object>())).Select(x => x.ToString()));
        var infraStackReferenceVpcId = infraStackReference.RequireOutput("VpcId");

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
            ProjectName = projectName,
            Environment = environment,
            IsProductionEnvironment = isProductionEnvironment,
            Tags = tags,
            VpcSetup = new VpcSetup()
            {
                VpcId = Output.Format($"{infraStackReferenceVpcId}"),
                PrivateSubnetIds = infraStackReferencePrivateSubnetIdsAsList as Output<IEnumerable<string>>
            }
        };

        var dbConfigs = new PostgresDatabase(config, stackSetup);
        var secretManagerSecret = new SecretsManager(config, stackSetup, dbConfigs);

        var lambdaFunctionConfigs = new LambdaFunctionUrl(config, stackSetup, secretManagerSecret);
        ApplicationUrl = lambdaFunctionConfigs.ApplicationUrl;
        LambdaId = lambdaFunctionConfigs.LambdaFunctionId;
        DBIdentifier = dbConfigs.DBIdentifier;

        var databaseMigratorLambda = new DatabaseMigratorLambda(config, stackSetup, secretManagerSecret);
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
            var value when value == Environments.Staging.ToString().ToLowerInvariant() => false,
            _ => false,
        };
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
    [Output] public Output<string>? DBIdentifier { get; set; }
    [Output] public Output<string>? DatabaseMigratorLambdaArn { get; set; }
}
