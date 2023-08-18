using System.Collections.Immutable;
using Pulumi;
using Pulumi.Awsx.Ec2;
using Pulumi.Awsx.Ec2.Inputs;

namespace VirtualFinland.UsersAPI.Deployment.Common.Models;

public class VpcSetup
{
    public VpcSetup(StackSetup stackSetup)
    {
        var vpc = new Vpc($"{stackSetup.ProjectName}-vf-vpc-{stackSetup.Environment}", new VpcArgs()
        {
            Tags = stackSetup.Tags,
            EnableDnsHostnames = true,
            NatGateways = new NatGatewayConfigurationArgs
            {
                Strategy = stackSetup.IsProductionEnvironment ? NatGatewayStrategy.OnePerAz : NatGatewayStrategy.Single
            }
        });

        this.VpcId = vpc.VpcId;
        this.PrivateSubnetIds = vpc.PrivateSubnetIds;
    }

    public Input<string>? VpcId = default!;
    public Output<ImmutableArray<string>> PrivateSubnetIds = default!;
}