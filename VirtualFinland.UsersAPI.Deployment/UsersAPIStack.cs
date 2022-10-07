using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Awsx.Ec2;
using Pulumi.Command.Local;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common;
using Instance = Pulumi.Aws.Rds.Instance;
using InstanceArgs = Pulumi.Aws.Rds.InstanceArgs;
using SubnetGroup = Pulumi.Aws.Dax.SubnetGroup;

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
                    { "ASPNETCORE_ENVIRONMENT", "Development" }
                }
            },
            Code = new FileArchive("../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/bin/Release/net6.0/VirtualFinland.UsersAPI.zip")
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

    private void InitializePostGresDatabase(Config config, InputMap<string> tags, bool isProductionEnvironment, Vpc vpc)
    {
        var dbSubNetGroup = new SubnetGroup("dbsubnets", new()
        {
            SubnetIds = vpc.PrivateSubnetIds, 
        });

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

            DbSubnetGroupName = dbSubNetGroup.Name,
            DbName = config.Require("dbName"),
            Username = config.Require("dbAdmin"),
            Password = password.Result,
            Tags = tags,
            SkipFinalSnapshot = !isProductionEnvironment, // DEV: For production set to FALSE to avoid accidental deletion of the cluster, data safety measure and is the default for AWS.
        });
    }

    [Output] public Output<string> Url { get; set; }
    }