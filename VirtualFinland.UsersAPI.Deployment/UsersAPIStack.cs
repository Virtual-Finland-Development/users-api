using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Linq;
using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Aws.S3;
using Pulumi.Command.Local;
using Pulumi.Random;
using VirtualFinland.UsersAPI.Deployment.Common;
using Instance = Pulumi.Aws.Rds.Instance;
using InstanceArgs = Pulumi.Aws.Rds.InstanceArgs;

namespace VirtualFinland.UsersAPI.Deployment;

public class UsersApiStack : Stack
{
    public UsersApiStack()
    {
        var config = new Config();
        bool isProductionEnvironment = Pulumi.Deployment.Instance.StackName == Environments.Prod.ToString().ToLowerInvariant();
        
        var stackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/{config.Require("infraStackReferenceName")}/{Pulumi.Deployment.Instance.StackName}");
        var stackReferencePrivateSubnetIds = stackReference.GetOutput("PrivateSubnetIds");
        var stackReferenceVpcId = stackReference.GetOutput("VpcId");

        InputMap<string> tags = new InputMap<string>()
        {
            {
                "Environment", config.Require("environment")
            },
            {
                "Project", config.Require("name")
            }
        };

        var privateSubnetIds = stackReferencePrivateSubnetIds.Apply(o => ((ImmutableArray<object>)(o ?? new ImmutableArray<object>())).Select(x => x.ToString()));

        var dbConfigs = InitializePostGresDatabase(config, tags, isProductionEnvironment, privateSubnetIds);

        var countriesCodeSetConfigs = UploadCountriesCodeSetData(tags);

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
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        var defaultSecurityGroup =Pulumi.Aws.Ec2.GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
        {
            VpcId = Output.Format($"{stackReferenceVpcId}")
        });
        
        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = defaultSecurityGroup.Apply(o=> $"{o.Id}"),
            SubnetIds = privateSubnetIds
        };

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
                    {
                        "ASPNETCORE_ENVIRONMENT", "Development"
                    },
                    {
                        "DB_CONNECTION", Output.All(dbConfigs.dbHostName, dbConfigs.dbPassword)
                            .Apply(pulumiOutputs => $"Host={pulumiOutputs[0]};Database={config.Require("dbName")};Username={config.Require("dbAdmin")};Password={pulumiOutputs[1]}")
                    },
                    {
                        "CODE_SET_COUNTRIES", Output.Format($"{countriesCodeSetConfigs}")
                    },
                }
            },
            Code = new FileArchive("../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/bin/Release/net6.0/VirtualFinland.UsersAPI.zip"),
            VpcConfig = functionVpcArgs
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

        this.ApplicationUrl = functionUrl.FunctionUrlResult;
        VpcId = Output.Format($"{stackReferenceVpcId}");
        this.PrivateSubNetIds = functionVpcArgs.SubnetIds;
        this.DefaultSecurityGroupId = defaultSecurityGroup.Apply(o=> $"{o.Id}");
    }

    private (Output<string> dbPassword, Output<string> dbHostName, Output<string> dbSubnetGroupName) InitializePostGresDatabase(Config config, InputMap<string> tags, bool isProductionEnvironment, InputList<string> privateSubNetIds)
    {
        var dbSubNetGroup = new Pulumi.Aws.Rds.SubnetGroup("dbsubnets", new()
        {

            SubnetIds = privateSubNetIds,

        });

        var password = new RandomPassword("password", new()
        {
            Length = 16,
            Special = false,
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
            PubliclyAccessible = !isProductionEnvironment, // DEV: For Production set to FALSE
            SkipFinalSnapshot = !isProductionEnvironment, // DEV: For production set to FALSE to avoid accidental deletion of the cluster, data safety measure and is the default for AWS.
        });

        this.DbPassword = password.Result;

        return (password.Result, rdsPostGreInstance.Address, rdsPostGreInstance.DbSubnetGroupName);
    }

    public Output<string> UploadCountriesCodeSetData(InputMap<string> tags)
    {
        var bucket = new Bucket("vf-users-api", new BucketArgs()
        {
            Tags = tags,
        });
        
        var bucketObject = new BucketObject("countries.json", new BucketObjectArgs
        {
            Bucket = bucket.BucketName,
            Source = new FileAsset("./Resources/countries.json"),
            Tags = tags,
            ContentType = "application/json",
            Acl = "public-read"
        });

        return CountriesCodeSetUrl = Output.Format($"https://{bucket.BucketDomainName}/countries.json");
    }

    [Output] public Output<string> ApplicationUrl { get; set; }

    [Output] public Output<ImmutableArray<string>> PrivateSubNetIds { get; set; }

    [Output] public Output<string> VpcId { get; set; }

    [Output] public Output<string> DefaultSecurityGroupId { get; set; }
    [Output] public Output<string> DbPassword { get; set; } = null!;
    
    [Output] public Output<string> CountriesCodeSetUrl { get; set; } = null!;
}