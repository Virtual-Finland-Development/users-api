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
        var infraStackReferencePrivateSubnetIds = infraStackReference.GetOutput("PrivateSubnetIds");
        var infraStackReferenceVpcId = infraStackReference.GetOutput("VpcId");
        VpcId = Output.Format($"{infraStackReferenceVpcId}");

        InputMap<string> tags = new InputMap<string>()
        {
            {
                "vfd:stack", Pulumi.Deployment.Instance.StackName
            },
            {
                "vfd:project", Pulumi.Deployment.Instance.ProjectName
            }
        };

        var privateSubnetIds = infraStackReferencePrivateSubnetIds.Apply(o => ((ImmutableArray<object>)(o ?? new ImmutableArray<object>())).Select(x => x.ToString()));
        PrivateSubnetIds = privateSubnetIds;

        var stackSetup = new StackSetup()
        {
            ProjectName = projectName,
            Environment = environment,
            IsProductionEnvironment = isProductionEnvironment,
            Tags = tags,
            VpcSetup = new VpcSetup()
            {
                VpcId = VpcId,
                PrivateSubnetIds = PrivateSubnetIds
            }
        };

        var dbConfigs = new PostgresDatabase(config, stackSetup, PrivateSubnetIds);
        DbIdentifier = dbConfigs.DbIdentifier;
        DbPassword = dbConfigs.DbPassword;

        var secretManagerSecret = new SecretsManager(config, stackSetup, dbConfigs);
        DbConnectionStringSecretId = secretManagerSecret.Name;

        var lambdaFunctionConfigs = new LambdaFunctionUrl(config, stackSetup, secretManagerSecret);
        ApplicationUrl = lambdaFunctionConfigs.ApplicationUrl;
        DefaultSecurityGroupId = lambdaFunctionConfigs.DefaultSecurityGroupId;
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
        public Output<string>? VpcId = default!;
        public Output<IEnumerable<string>> PrivateSubnetIds = default!;
    }

    // Outputs for Pulumi service
    [Output] public Output<string>? ApplicationUrl { get; set; }
    [Output] public Output<IEnumerable<string>> PrivateSubnetIds { get; set; }
    [Output] public Output<string>? VpcId { get; set; }
    [Output] public Output<string>? DefaultSecurityGroupId { get; set; }
    [Output] public Output<string> DbPassword { get; set; } = null!;
    [Output] public Output<string>? DbConnectionStringSecretId { get; set; }
    [Output] public Output<string>? DbIdentifier { get; set; }
    [Output] public Output<string>? LambdaId { get; set; }
}
