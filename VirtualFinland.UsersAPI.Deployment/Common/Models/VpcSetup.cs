using System.Collections.Generic;
using Pulumi;

namespace VirtualFinland.UsersAPI.Deployment.Common.Models;

public record VpcSetup
{
    public Input<string> VpcId = default!;
    public Output<IEnumerable<string>> PrivateSubnetIds = default!;
    public Input<string> SecurityGroupId = default!;
}