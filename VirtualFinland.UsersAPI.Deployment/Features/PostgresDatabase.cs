using Pulumi;
using Pulumi.Aws.Kms;
using Pulumi.Aws.Rds;
using Pulumi.Aws.Rds.Inputs;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Instance = Pulumi.Aws.Rds.Instance;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database
/// </summary>
public class PostgresDatabase
{
    public PostgresDatabase(Config config, StackSetup stackSetup)
    {
        if (stackSetup.IsProductionEnvironment)
        {
            SetupProductionPostgresDatabase(config, stackSetup);
        }
        else
        {
            SetupDevelopmentPostgresDatabase(config, stackSetup);
        }

        if (config.GetBoolean("useRdsProxy") == true)
        {
            var rdsProxy = new RDSProxy(config, stackSetup, this);
            DatabaseConnectionString = rdsProxy.DatabaseConnectionString; // Override the connection string with one from the proxy
        }
    }

    /// <summary>
    ///  Setup AWS Aurora RDS Serverless V2 for postgresql
    /// </summary>
    public void SetupProductionPostgresDatabase(Config config, StackSetup stackSetup)
    {
        var dbSubNetGroup = new SubnetGroup(stackSetup.CreateResourceName("database-subnets"), new()
        {
            SubnetIds = stackSetup.VpcSetup.PrivateSubnetIds,
        });

        var password = new RandomPassword(stackSetup.CreateResourceName("database-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });

        // Encryption key (KMS)
        var encryptionKey = new Key(stackSetup.CreateResourceName("database-encryption-key"), new()
        {
            Description = "Encryption key for the database",
            Tags = stackSetup.Tags,
            DeletionWindowInDays = 30, // On deletion, the key will be retained for 30 days before being deleted permanently
        });

        // AWS Aurora RDS Serverless V2 for postgresql
        var auroraCluster = new Cluster(stackSetup.CreateResourceName("database-cluster"), new ClusterArgs()
        {
            Engine = "aurora-postgresql",
            EngineVersion = "13.6",
            EngineMode = "provisioned", // serverless v2
            Serverlessv2ScalingConfiguration = new ClusterServerlessv2ScalingConfigurationArgs
            {
                MaxCapacity = 4,
                MinCapacity = 0.5,
            },

            DatabaseName = config.Require("dbName"),
            MasterUsername = config.Require("dbAdmin"),
            MasterPassword = password.Result,

            SkipFinalSnapshot = false,
            DeletionProtection = true,
            StorageEncrypted = true,
            KmsKeyId = encryptionKey.Arn,

            DbSubnetGroupName = dbSubNetGroup.Name,
            Tags = stackSetup.Tags,
        });

        new ClusterInstance(stackSetup.CreateResourceName("database-instance"), new()
        {
            ClusterIdentifier = auroraCluster.ClusterIdentifier,
            InstanceClass = "db.serverless",
            Engine = "aurora-postgresql",
            EngineVersion = auroraCluster.EngineVersion,
            Tags = stackSetup.Tags,
        });

        var DbName = config.Require("dbName");
        var DbUsername = config.Require("dbAdmin");
        var DbHostName = auroraCluster.Endpoint;
        var DbPassword = password.Result;

        DatabaseConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DBIdentifier = auroraCluster.ClusterIdentifier;
    }

    /// <summary>
    /// Setup AWS RDS for postgresql
    /// </summary>
    public void SetupDevelopmentPostgresDatabase(Config config, StackSetup stackSetup)
    {
        var dbSubNetGroup = new SubnetGroup("dbsubnets", new()
        {
            SubnetIds = stackSetup.VpcSetup.PrivateSubnetIds,
        });

        var password = new RandomPassword("password", new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });

        var rdsPostGreInstance = new Instance(stackSetup.CreateResourceName("postgres-db"), new InstanceArgs()
        {
            Engine = "postgres",
            InstanceClass = "db.t3.micro",
            AllocatedStorage = 20,

            DbSubnetGroupName = dbSubNetGroup.Name,
            DbName = config.Require("dbName"),
            Username = config.Require("dbAdmin"),
            Password = password.Result,
            Tags = stackSetup.Tags,
            PubliclyAccessible = false,
            SkipFinalSnapshot = true,
        });

        var DbName = config.Require("dbName");
        var DbUsername = config.Require("dbAdmin");
        var DbHostName = rdsPostGreInstance.Endpoint;
        var DbPassword = password.Result;
        DatabaseConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DBIdentifier = rdsPostGreInstance.Identifier;
    }

    public SecretsManager SecretsManager = default!;
    public Output<string> DBIdentifier = default!;
    public Output<string> DatabaseConnectionString = default!;
}