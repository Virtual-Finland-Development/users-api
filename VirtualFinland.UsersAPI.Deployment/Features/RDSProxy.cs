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
    public RDSProxy(Config config, StackSetup stackSetup, PostgresDatabase database)
    {
        // RDS proxy access secret
        var username = new RandomPassword(stackSetup.CreateResourceName("rdsproxy-username"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });
        var password = new RandomPassword(stackSetup.CreateResourceName("rdsproxy-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });
        var rdsProxySecretString = Output.Format($"{{\"username\":\"{username.Result}\",\"password\":\"{password.Result}\"}}");
        var rdsProxySecret = new SecretsManager(config, stackSetup, "rdsProxySecret", rdsProxySecretString);

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

        new RolePolicyAttachment($"{stackSetup.ProjectName}-RdsProxy-SecretManager-{stackSetup.Environment}", new RolePolicyAttachmentArgs
        {
            Role = rdsProxyRole.Name,
            PolicyArn = rdsProxySecret.Arn
        });

        // AWS RDS Proxy
        var rdsProxy = new Proxy(stackSetup.CreateResourceName("database-proxy"), new()
        {
            DebugLogging = false,
            EngineFamily = "POSTGRESQL",
            RequireTls = true,
            RoleArn = rdsProxyRole.Arn,
            VpcSubnetIds = stackSetup.VpcSetup.PrivateSubnetIds,
            VpcSecurityGroupIds = new[] { stackSetup.VpcSetup.SecurityGroupId },
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

        // RDS Proxy Target
        new ProxyTarget(stackSetup.CreateResourceName("database-proxy-target"), new ProxyTargetArgs()
        {
            DbProxyName = rdsProxy.Name,
            DbInstanceIdentifier = database.DBIdentifier,
        });

        // Set outputs
        ProxyEndpoint = rdsProxy.Endpoint;
        ProxyIdentifier = rdsProxy.Id;

        var DbName = config.Require("dbName");
        DatabaseConnectionString = Output.Format($"Host={rdsProxy.Endpoint};Database={DbName};Username={username.Result};Password={password.Result}");
    }

    [Output]
    public Output<string> ProxyEndpoint { get; set; }
    public Output<string> ProxyIdentifier { get; set; }
    public Output<string> DatabaseConnectionString { get; set; }
}