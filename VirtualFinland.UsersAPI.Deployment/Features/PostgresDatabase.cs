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
    public PostgresDatabase(Config config, StackSetup stackSetup)
    {
        var dbSubNetGroup = new Pulumi.Aws.Rds.SubnetGroup("dbsubnets", new()
        {
            SubnetIds = stackSetup.VpcSetup.PrivateSubnetIds,
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
        });

        DbHostName = rdsPostGreInstance.Endpoint;
        DbIdentifier = rdsPostGreInstance.Identifier;
        DbPassword = password.Result;

        DbName = config.Require("dbName");
        DbUsername = config.Require("dbAdmin");
    }

    public Output<string> DbPassword = default!;
    public Output<string> DbHostName = default!;
    public Output<string> DbSubnetGroupName = default!;
    public Output<string> DbIdentifier = default!;
    public string DbName = default!;
    public string DbUsername = default!;
}