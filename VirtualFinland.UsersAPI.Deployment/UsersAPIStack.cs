using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Command.Local;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common;
using Instance = Pulumi.Aws.Rds.Instance;
using InstanceArgs = Pulumi.Aws.Rds.InstanceArgs;
using SubnetGroup = Pulumi.Aws.Dax.SubnetGroup;
using Vpc = Pulumi.Awsx.Ec2.Vpc;

namespace VirtualFinland.UsersAPI.Deployment;

public class UsersAPIStack : Stack
{
    public UsersAPIStack()
    {
        var config = new Config();
        bool isProductionEnvironment = config.Require("environment") == Environments.Prod.ToString().ToLowerInvariant();
        
        // var stackReference = new StackReference("adriansimionescuRebase/infrastructure/dev");
        // var vpcId = stackReference.GetOutput("VpcId");
        
        InputMap<string> tags = new InputMap<string>()
        {
            { "Environment", config.Require("environment") },
            { "Project", config.Require("name") }
        };
        
        var dbConfigs = InitializePostGresDatabase(config, tags, isProductionEnvironment, null);

        var role = new Role("vf-UsersAPI-LambdaRole", new RoleArgs
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
                                    { "Service", "lambda.amazonaws.com" }
                                }
                            }
                        }
                    }
                }
            })
        });

        var rolePolicyAttachment = new RolePolicyAttachment("vf-UsersAPI-LambdaRoleAttachment", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{role.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaBasicExecutionRole.ToString()
        });

        /*var default_vpc = new Pulumi.Aws.Ec2.DefaultVpc("default");

        var defaultSecurityGroup = Pulumi.Aws.Ec2.GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
        {
            Name = "default",
            VpcId = default_vpc.Id
        });

        var defaultSubnet = new Pulumi.Aws.Ec2.DefaultSubnet("default", new DefaultSubnetArgs()
        {
            AvailabilityZone = "eu-north-1"
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            VpcId = default_vpc.Id,
            SecurityGroupIds = defaultSecurityGroup.Apply( o => o.Id),
            SubnetIds = defaultSubnet.Id
        };*/

        var lambdaFunction = new Function("vf-UsersAPI", new FunctionArgs
        {
            Role = role.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 30,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    { "ASPNETCORE_ENVIRONMENT", "Development" },
                    { "DB_CONNECTION", Output.All(dbConfigs.dbHostName, dbConfigs.dbPassword)
                        .Apply(pulumiOutputs =>$"Host={pulumiOutputs[0]};Database={config.Require("dbName")};Username={config.Require("dbAdmin")};Password={pulumiOutputs[1]}") }
                }
            },
            Code = new FileArchive("../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/bin/Release/net6.0/VirtualFinland.UsersAPI.zip"),
            //VpcConfig = functionVpcArgs
        });

        var functionUrl = new FunctionUrl("vf-UsersAPI-FunctionUrl", new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.Arn,
            AuthorizationType = "NONE"
        });

        var localCommand = new Command("vf-UsersAPI-AddPermissions", new CommandArgs
        {
            Create = Output.Format(
                $"aws lambda add-permission --function-name {lambdaFunction.Arn} --action lambda:InvokeFunctionUrl --principal '*' --function-url-auth-type NONE --statement-id FunctionUrlAllowAccess")
        }, new CustomResourceOptions
        {
            DeleteBeforeReplace = true,
            DependsOn = new InputList<Resource>
            {
                lambdaFunction
            }
        });

        Url = functionUrl.FunctionUrlResult;
    }

    private (Output<string> dbPassword, Output<string> dbHostName, Output<string> dbSubnetGroupName) InitializePostGresDatabase(Config config, InputMap<string> tags, bool isProductionEnvironment, Vpc vpc)
    {
        /*var dbSubNetGroup = new SubnetGroup("dbsubnets", new()
        {
            SubnetIds = vpc.PrivateSubnetIds, 
        });*/

        var password = new RandomPassword("password", new()
        {
            Length = 16,
            Special = true,
            OverrideSpecial = "_%@",
        });

        var rdsPostGreInstance = new Instance("postgres-db", new InstanceArgs()
        {
            Engine = "postgres",
            InstanceClass = "db.t3.micro",
            AllocatedStorage = 20,

            //DbSubnetGroupName = dbSubNetGroup.Name,
            DbName = config.Require("dbName"),
            Username = config.Require("dbAdmin"),
            Password = password.Result,
            Tags = tags,
            PubliclyAccessible = !isProductionEnvironment, 
            SkipFinalSnapshot = !isProductionEnvironment, // DEV: For production set to FALSE to avoid accidental deletion of the cluster, data safety measure and is the default for AWS.
        });

        DBPassword = password.Result;

        return (password.Result, rdsPostGreInstance.Address, rdsPostGreInstance.DbSubnetGroupName);
    }

    [Output] public Output<string> Url { get; set; }
    [Output] public Output<string> DBPassword { get; set; }
    }