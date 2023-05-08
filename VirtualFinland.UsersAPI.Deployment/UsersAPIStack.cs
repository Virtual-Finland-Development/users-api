using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common;
using VirtualFinland.UsersAPI.Deployment.Features;

namespace VirtualFinland.UsersAPI.Deployment;

public class UsersApiStack : Stack
{
    public UsersApiStack()
    {
        var config = new Config();
        bool isProductionEnvironment = Pulumi.Deployment.Instance.StackName == Environments.Prod.ToString().ToLowerInvariant();
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
        DbIdentifier = dbConfigs.DbIdentifier;

        var secretManagerSecret = new SecretsManager(config, stackSetup, dbConfigs);
        DbConnectionStringSecretId = secretManagerSecret.Name;

        var lambdaFunctionConfigs = new LambdaFunctionUrl(config, stackSetup, secretManagerSecret);
        ApplicationUrl = lambdaFunctionConfigs.ApplicationUrl;
        LambdaId = lambdaFunctionConfigs.LambdaFunctionArn;
    }

    public record StackSetup
    {
        public InputMap<string> Tags = default!;
        public bool IsProductionEnvironment;
        public string Environment = default!;
        public string ProjectName = default!;
        public VpcSetup VpcSetup = default!;
    }

    public record VpcSetup
    {
        public Input<string>? VpcId = default!;
        public Output<IEnumerable<string>> PrivateSubnetIds = default!;
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<string>? DbConnectionStringSecretId { get; set; }
    [Output] public Output<string>? DbIdentifier { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
}
