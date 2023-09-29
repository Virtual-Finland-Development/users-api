using Pulumi;
using Pulumi.Aws.Kms;
using Pulumi.Aws.Rds;
using Pulumi.Aws.Rds.Inputs;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Instance = Pulumi.Aws.Rds.Instance;
using Provider = Pulumi.PostgreSql.Provider;
namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database
/// </summary>
public class PostgresDatabase
{
    public PostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup)
    {
        if (stackSetup.IsProductionEnvironment)
        {
            SetupProductionPostgresDatabase(config, stackSetup, vpcSetup);
        }
        else
        {
            SetupDevelopmentPostgresDatabase(config, stackSetup, vpcSetup);
        }

        if (config.GetBoolean("useRdsProxy") == true)
        {
            var rdsProxy = new RDSProxy(config, stackSetup, this, vpcSetup);
            DatabaseConnectionString = rdsProxy.DatabaseConnectionString; // Override the connection string with one from the proxy
        }
    }

    /// <summary>
    ///  Setup AWS Aurora RDS Serverless V2 for postgresql
    /// </summary>
    public void SetupProductionPostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup)
    {
        var dbSubNetGroup = new SubnetGroup(stackSetup.CreateResourceName("database-subnets"), new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
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

        var DbUsername = config.Require("dbUser");
        var DbPassword = new RandomPassword(stackSetup.CreateResourceName("database-user-password"), new()
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
            EngineVersion = "13.6",
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
            DeletionProtection = true,
            StorageEncrypted = true,
            KmsKeyId = encryptionKey.Arn,

            DbSubnetGroupName = dbSubNetGroup.Name,
            Tags = stackSetup.Tags,
            BackupRetentionPeriod = 7, // @TODO: Define for production
        });

        var dbInstanceIdentifier = stackSetup.CreateResourceName("database-instance");
        _ = new ClusterInstance(dbInstanceIdentifier, new()
        {
            Identifier = dbInstanceIdentifier,
            ClusterIdentifier = auroraCluster.ClusterIdentifier,
            InstanceClass = "db.serverless",
            Engine = "aurora-postgresql",
            EngineVersion = auroraCluster.EngineVersion,
            Tags = stackSetup.Tags,
        });

        var DbHostName = auroraCluster.Endpoint;

        SetupDatabaseUser(stackSetup, DbHostName, DbName, DbAdminUsername, DbAdminPassword, DbUsername, DbPassword);

        DatabaseConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DatabaseAdminConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbAdminUsername};Password={DbAdminPassword}");
        DBIdentifier = auroraCluster.ClusterIdentifier;
    }

    /// <summary>
    /// Setup AWS RDS for postgresql
    /// </summary>
    public void SetupDevelopmentPostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup)
    {
        var dbSubNetGroup = new SubnetGroup(stackSetup.CreateResourceName("dbsubnets"), new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
        });

        var DbName = config.Require("dbName");
        var DbAdminUsername = config.Require("dbAdminUser");
        var DbAdminPassword = new RandomPassword(stackSetup.CreateResourceName("database-admin-password"), new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        }).Result;

        var DbUsername = config.Require("dbUser");
        var DbPassword = new RandomPassword(stackSetup.CreateResourceName("database-user-password"), new()
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
            SkipFinalSnapshot = true
        });

        var DbHostName = rdsPostgreSqlInstance.Endpoint;

        SetupDatabaseUser(stackSetup, DbHostName, DbName, DbAdminUsername, DbAdminPassword, DbUsername, DbPassword);

        DatabaseConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbUsername};Password={DbPassword}");
        DatabaseAdminConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbAdminUsername};Password={DbAdminPassword}");
        DBIdentifier = rdsPostgreSqlInstance.Identifier;
    }

    /// <summary>
    /// Setup the database user
    /// </summary>
    private static void SetupDatabaseUser(StackSetup stackSetup, Output<string> DbHostName, string DbName, string DbAdminUsername, Output<string> DbAdminPassword, string DbUsername, Output<string> DbPassword)
    {
        var postgresProvider = new Provider(stackSetup.CreateResourceName("postgres-provider"), new()
        {
            Host = DbHostName,
            Username = DbAdminUsername,
            Password = DbAdminPassword,
        });
        var database = new Pulumi.PostgreSql.Database(stackSetup.CreateResourceName("postgres-database"), new()
        {
            Name = DbName,
        }, new()
        {
            Provider = postgresProvider,
        });
        var user = new Pulumi.PostgreSql.Role(stackSetup.CreateResourceName("database-user"), new()
        {
            Name = DbUsername,
            Password = DbPassword,
        }, new()
        {
            Provider = postgresProvider,
        });
        _ = new Pulumi.PostgreSql.Grant(stackSetup.CreateResourceName("database-user-grant"), new()
        {
            Database = database.Name,
            Role = user.Name,
            Privileges = new[] { "SELECT", "INSERT", "UPDATE", "DELETE" },
            ObjectType = "ALL IN SCHEMA public",
        }, new()
        {
            Provider = postgresProvider,
        });
    }

    public Output<string> DBIdentifier = default!;
    public Output<string> DatabaseConnectionString = default!;
    public Output<string> DatabaseAdminConnectionString = default!;
}