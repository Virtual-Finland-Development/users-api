using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Pulumi.Aws.Iam;
using System.Text.Json;
using System.Collections.Generic;
using Pulumi.Aws.Rds;
using Pulumi.Aws.Rds.Inputs;
using Pulumi.Random;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class RDSProxy
{
    public RDSProxy(Config config, StackSetup stackSetup, PostgresDatabase database, VpcSetup vpcSetup)
    {
        // RDS proxy access secret
        var dbName = config.Require("dbName");
        var dbUsername = config.Require("dbUser");

        var password = new RandomPassword(stackSetup.CreateResourceName("rdsproxy-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });
        var rdsProxySecretString = Output.Format($"{{\"username\":\"{dbUsername}\",\"password\":\"{password.Result}\"}}");
        var rdsProxySecret = new SecretsManager(stackSetup, "rdsProxySecret", rdsProxySecretString);

        // Create role for rds proxy
        var rdsProxyRole = new Role(stackSetup.CreateResourceName("database-proxy-role"), new RoleArgs()
        {
            AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                { "Version", "2012-10-17" },
                {
                    "Statement", new[]
                    {
                        new Dictionary<string, object?>
                        {
                            { "Action", "sts:AssumeRole" },
                            { "Effect", "Allow" },
                            { "Sid", "" },
                            {
                                "Principal", new Dictionary<string, object?>
                                {
                                    { "Service", "rds.amazonaws.com" }
                                }
                            }
                        }
                    }
                }
            }),
            Tags = stackSetup.Tags
        });

        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("RdsProxy-SecretManager"), new RolePolicyAttachmentArgs
        {
            Role = rdsProxyRole.Name,
            PolicyArn = rdsProxySecret.ReadPolicy.Arn,
        });

        // AWS RDS Proxy
        var rdsProxy = new Proxy(stackSetup.CreateResourceName("database-proxy"), new()
        {
            DebugLogging = false,
            EngineFamily = "POSTGRESQL",
            RequireTls = true,
            RoleArn = rdsProxyRole.Arn,
            VpcSubnetIds = vpcSetup.PrivateSubnetIds,
            VpcSecurityGroupIds = new[] { vpcSetup.SecurityGroupId },
            Auths = new[] {
                new ProxyAuthArgs
                {
                    AuthScheme = "SECRETS",
                    Description = "Secrets authentication",
                    SecretArn = rdsProxySecret.Arn,
                    IamAuth = "DISABLED"
                }
            },
            Tags = stackSetup.Tags,
        });

        // Target group
        var rdsProxyTargetGroup = new ProxyDefaultTargetGroup(stackSetup.CreateResourceName("database-proxy-target-group"), new()
        {
            DbProxyName = rdsProxy.Name,
            ConnectionPoolConfig = new ProxyDefaultTargetGroupConnectionPoolConfigArgs
            {
                MaxConnectionsPercent = 100,
                MaxIdleConnectionsPercent = 50,
                ConnectionBorrowTimeout = 120,
            },
        });

        // RDS Proxy Target
        _ = new ProxyTarget(stackSetup.CreateResourceName("database-proxy-target"), new ProxyTargetArgs()
        {
            DbProxyName = rdsProxy.Name,
            DbClusterIdentifier = database.DBClusterIdentifier,
            TargetGroupName = rdsProxyTargetGroup.Name,
        });

        // Set outputs
        ProxyEndpoint = rdsProxy.Endpoint;
        ProxyIdentifier = rdsProxy.Id;

        ProxyConnectionString = Output.Format($"Host={rdsProxy.Endpoint};Database={dbName};Username={dbUsername};Password={password.Result}");
        ProxyUsername = dbUsername;
        ProxyPassword = password.Result;
    }

    [Output]
    public Output<string> ProxyEndpoint { get; set; }
    public Output<string> ProxyIdentifier { get; set; }
    public Output<string> ProxyConnectionString { get; set; }
    public string ProxyUsername { get; set; }
    public Output<string> ProxyPassword { get; set; }
}