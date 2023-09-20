using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Pulumi.Aws.ElastiCache;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class RedisElastiCache
{
    public RedisElastiCache(StackSetup stackSetup, VpcSetup vpcSetup)
    {
        var elastiCacheSubnet = new SubnetGroup(stackSetup.CreateResourceName("RedisElastiCacheSubnet"), new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
            Tags = stackSetup.Tags,
        });

        var cluster = new Cluster(stackSetup.CreateResourceName("RedisElastiCache"), new()
        {
            Engine = "redis",
            EngineVersion = "6.x",
            NodeType = "cache.t3.micro",
            NumCacheNodes = 1,
            ParameterGroupName = "default.redis6.x",
            Port = 6379,
            ReplicationGroupId = stackSetup.CreateResourceName("RedisElastiCache"),
            SecurityGroupIds = new[] { vpcSetup.SecurityGroupId },
            SubnetGroupName = elastiCacheSubnet.Name,
            Tags = stackSetup.Tags,
        });

        ClusterArn = cluster.Arn;
        ClusterEndpoint = cluster.CacheNodes.Apply(nodes => nodes[0].Address);
    }

    public Output<string> ClusterArn = default!;
    public Output<string?> ClusterEndpoint = default!;
}