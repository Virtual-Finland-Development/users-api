using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Kms;
using Pulumi.Aws.Rds;
using Pulumi.Aws.Rds.Inputs;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Instance = Pulumi.Aws.Rds.Instance;
using System.Text.Json;
using Pulumi.Aws.Lambda;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database
/// </summary>
public class PostgresDatabase
{
    public PostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup, CloudWatch cloudwatch)
    {
        if (stackSetup.IsProductionEnvironment)
        {
            SetupProductionPostgresDatabase(config, stackSetup, vpcSetup, cloudwatch);
        }
        else
        {
            SetupDevelopmentPostgresDatabase(config, stackSetup, vpcSetup, cloudwatch);
        }

        if (config.GetBoolean("useRdsProxy") == true)
        {
            var rdsProxy = new RDSProxy(config, stackSetup, this, vpcSetup);
            DatabaseConnectionString = rdsProxy.ProxyConnectionString; // Override the connection string with one from the proxy
            DbUsername = rdsProxy.ProxyUsername; // Also override the username and password for the InvokeInitialDatabaseUserSetupFunction trigger
            DbPassword = rdsProxy.ProxyPassword;
            MainResource = rdsProxy.MainResource; // Wait for the proxy to be ready where needed, instead of database ref
        }
    }

    /// <summary>
    ///  Setup AWS Aurora RDS Serverless V2 for postgresql
    /// </summary>
    public void SetupProductionPostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup, CloudWatch cloudwatch)
    {
        var dbSubNetGroup = new SubnetGroup(stackSetup.CreateResourceName("database-subnets"), new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
            Tags = stackSetup.Tags,
        });

        // Encryption key (KMS)
        var encryptionKey = new Key(stackSetup.CreateResourceName("database-encryption-key"), new()
        {
            Description = "Encryption key for the database",
            Tags = stackSetup.Tags,
            DeletionWindowInDays = 30, // On deletion, the key will be retained for 30 days before being deleted permanently
        });

        var DbName = config.Require("dbName");
        var DbAdminUsername = config.Require("dbAdminUser");
        var DbAdminPassword = new RandomPassword(stackSetup.CreateResourceName("database-admin-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        }).Result;

        DbUsername = config.Require("dbUser");
        DbPassword = new RandomPassword(stackSetup.CreateResourceName("database-user-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        }).Result;

        // AWS Aurora RDS Serverless V2 for postgresql
        var clusterIdentifier = stackSetup.CreateResourceName("database-cluster");
        var auroraCluster = new Cluster(clusterIdentifier, new ClusterArgs()
        {
            ClusterIdentifier = clusterIdentifier,
            Engine = "aurora-postgresql",
            EngineVersion = "13.8",
            EngineMode = "provisioned", // serverless v2
            Serverlessv2ScalingConfiguration = new ClusterServerlessv2ScalingConfigurationArgs
            {
                MaxCapacity = 4,
                MinCapacity = 0.5,
            },

            DatabaseName = DbName,
            MasterUsername = DbAdminUsername,
            MasterPassword = DbAdminPassword,

            SkipFinalSnapshot = false,
            FinalSnapshotIdentifier = $"{clusterIdentifier}-final-snapshot",
            DeletionProtection = true,
            StorageEncrypted = true,
            KmsKeyId = encryptionKey.Arn,

            DbSubnetGroupName = dbSubNetGroup.Name,
            Tags = stackSetup.Tags,
            BackupRetentionPeriod = 7, // @TODO: Define for production
            EnabledCloudwatchLogsExports = new[] { "postgresql" },
        });

        var dbInstanceIdentifier = stackSetup.CreateResourceName("database-instance");
        var dbInstance = new ClusterInstance(dbInstanceIdentifier, new()
        {
            Identifier = dbInstanceIdentifier,
            ClusterIdentifier = auroraCluster.ClusterIdentifier,
            InstanceClass = "db.serverless",
            Engine = "aurora-postgresql",
            EngineVersion = auroraCluster.EngineVersion,
            Tags = stackSetup.Tags,
        });

        var DbEndpoint = auroraCluster.Endpoint;
        DatabaseConnectionString = Output.Format($"Host={DbEndpoint};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DatabaseAdminConnectionString = Output.Format($"Host={DbEndpoint};Database={DbName};Username={DbAdminUsername};Password={DbAdminPassword}");
        DBIdentifier = dbInstance.Identifier;
        DBClusterIdentifier = auroraCluster.ClusterIdentifier;
        IsDatabaseCluster = true;
        MainResource = auroraCluster;

        LogGroup = cloudwatch.CreateLogGroup(stackSetup, "database", Output.Format($"/aws/rds/cluster/{auroraCluster.ClusterIdentifier}/postgresql"), 3);
    }

    /// <summary>
    /// Setup AWS RDS for postgresql
    /// </summary>
    public void SetupDevelopmentPostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup, CloudWatch cloudwatch)
    {
        var dbSubNetGroup = new SubnetGroup(stackSetup.CreateResourceName("dbsubnets"), new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
            Tags = stackSetup.Tags,
        });

        var DbName = config.Require("dbName");
        var DbAdminUsername = config.Require("dbAdminUser");
        var DbAdminPassword = new RandomPassword(stackSetup.CreateResourceName("database-admin-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        }).Result;

        DbUsername = config.Require("dbUser");
        DbPassword = new RandomPassword(stackSetup.CreateResourceName("database-user-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        }).Result;

        var rdsPostgreSqlInstance = new Instance(stackSetup.CreateResourceName("postgres-db"), new InstanceArgs()
        {
            Engine = "postgres",
            InstanceClass = "db.t3.micro",
            AllocatedStorage = 20,

            DbSubnetGroupName = dbSubNetGroup.Name,
            Username = DbAdminUsername,
            Password = DbAdminPassword,
            Tags = stackSetup.Tags,
            PubliclyAccessible = false,
            SkipFinalSnapshot = true,
            EnabledCloudwatchLogsExports = new[] { "postgresql" },
        });

        var DbEndpoint = rdsPostgreSqlInstance.Endpoint;
        DatabaseConnectionString = Output.Format($"Host={DbEndpoint};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DatabaseAdminConnectionString = Output.Format($"Host={DbEndpoint};Database={DbName};Username={DbAdminUsername};Password={DbAdminPassword}");
        DBIdentifier = rdsPostgreSqlInstance.Identifier;
        DBClusterIdentifier = Output.Create("");
        IsDatabaseCluster = false;
        MainResource = rdsPostgreSqlInstance;

        LogGroup = cloudwatch.CreateLogGroup(stackSetup, "database", Output.Format($"/aws/rds/instance/{rdsPostgreSqlInstance.Identifier}/postgresql"), 3);
    }

    /// <summary>
    /// Setup the database user
    /// </summary>
    public void InvokeInitialDatabaseUserSetupFunction(StackSetup stackSetup, Function adminFunction)
    {
        DbPassword.Apply(
            password =>
            {
                var invokePayload = JsonSerializer.Serialize(new
                {
                    action = "InitializeDatabaseUser",
                    data = JsonSerializer.Serialize(new
                    {
                        Username = DbUsername,
                        Password = password,
                    }),
                });

                _ = new Pulumi.Command.Local.Command(stackSetup.CreateResourceName("InitialDatabaseUserSetup"), new()
                {
                    Create = Output.Format($"aws lambda invoke --payload '{invokePayload}' --cli-binary-format raw-in-base64-out --function-name {adminFunction.Arn} /dev/null"),
                    Triggers = new InputList<string>
                    {
                        DbPassword,
                        Output.Create(DbUsername),
                    }
                }, new() { DependsOn = new[] { adminFunction, MainResource } });
                return password;
            }
        );
    }

    public void InvokeInitialDatabaseAuditLogTriggersSetupFunction(StackSetup stackSetup, Function adminFunction)
    {
        var invokePayload = JsonSerializer.Serialize(new
        {
            action = "InitializeDatabaseAuditLogTriggers",
        });

        _ = new Pulumi.Command.Local.Command(stackSetup.CreateResourceName("InitializeDatabaseAuditLogTriggers"), new()
        {
            Create = Output.Format($"aws lambda invoke --payload '{invokePayload}' --cli-binary-format raw-in-base64-out --function-name {adminFunction.Arn} /dev/null"),
        }, new() { DependsOn = new[] { adminFunction, MainResource } });
    }

    public Output<string> DBIdentifier = default!;
    public Output<string> DBClusterIdentifier = default!;
    public Resource DatabaseResource = default!;
    public Resource MainResource = default!; // To wait for the database to be ready
    public bool IsDatabaseCluster = false;
    public string DbUsername = default!;
    public Output<string> DbPassword = default!;
    public Output<string> DatabaseConnectionString = default!;
    public Output<string> DatabaseAdminConnectionString = default!;
    public LogGroup LogGroup = default!;
}