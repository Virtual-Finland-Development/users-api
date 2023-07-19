using Pulumi;
using Pulumi.Aws.Rds;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Instance = Pulumi.Aws.Rds.Instance;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database
/// </summary>
public class PostgresDatabase
{
    public PostgresDatabase(Config config, StackSetup stackSetup, VpcSetup vpcSetup)
    {
        var dbSubNetGroup = new Pulumi.Aws.Rds.SubnetGroup("dbsubnets", new()
        {
            SubnetIds = vpcSetup.PrivateSubnetIds,
        });

        var password = new RandomPassword("password", new()
        {
            Length = 16,
            Special = false,
            OverrideSpecial = "_%@",
        });

        var rdsPostGreInstance = new Instance($"{stackSetup.ProjectName}-postgres-db-{stackSetup.Environment}", new InstanceArgs()
        {
            Engine = "postgres",
            InstanceClass = "db.t3.micro",
            AllocatedStorage = 20,

            DbSubnetGroupName = dbSubNetGroup.Name,
            DbName = config.Require("dbName"),
            Username = config.Require("dbAdmin"),
            Password = password.Result,
            Tags = stackSetup.Tags,
            PubliclyAccessible = !stackSetup.IsProductionEnvironment, // DEV: For Production set to FALSE
            SkipFinalSnapshot = !stackSetup.IsProductionEnvironment, // DEV: For production set to FALSE to avoid accidental deletion of the cluster, data safety measure and is the default for AWS.
            //SnapshotIdentifier = "" // See README.database.md for more information
        });

        var DbName = config.Require("dbName");
        var DbUsername = config.Require("dbAdmin");
        var DbHostName = rdsPostGreInstance.Endpoint;
        DBIdentifier = rdsPostGreInstance.Identifier;
        var DbPassword = password.Result;
        DbConnectionString = Output.Format($"Host={DbHostName};Database={DbName};Username={DbUsername};Password={DbPassword}");
    }

    public Output<string> DbConnectionString = default!;
    public Output<string> DBIdentifier = default!;
}