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
        var environment = Pulumi.Deployment.Instance.StackName;
        var projectName = Pulumi.Deployment.Instance.ProjectName;

        var stackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/{config.Require("infraStackReferenceName")}/{environment}");
        var stackReferencePrivateSubnetIds = stackReference.GetOutput("PrivateSubnetIds");
        var stackReferenceVpcId = stackReference.GetOutput("VpcId");

        var codeSetStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/codesets/{environment}");
        var codesetsEndpointUrl = codeSetStackReference.GetOutput("url");


        InputMap<string> tags = new InputMap<string>()
        {
            {
                "vfd:stack", Pulumi.Deployment.Instance.StackName
            },
            {
                "vfd:project", Pulumi.Deployment.Instance.ProjectName
            }
        };

        var privateSubnetIds = stackReferencePrivateSubnetIds.Apply(o => ((ImmutableArray<object>)(o ?? new ImmutableArray<object>())).Select(x => x.ToString()));

        var dbConfigs = InitializePostGresDatabase(config, tags, isProductionEnvironment, privateSubnetIds, environment, projectName);

        var bucket = CreateBucket(tags, environment, projectName);

        var role = new Role($"{projectName}-LambdaRole-{environment}", new RoleArgs
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

        var rolePolicyAttachment = new RolePolicyAttachment($"{projectName}-LambdaRoleAttachment-{environment}", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{role.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        var secretManagerReadPolicy = new Pulumi.Aws.Iam.Policy($"{projectName}-LambdaSecretManagerPolicy-{environment}", new()
        {
            Path = "/",
            Description = "Users-API Secret Get Policy",
            PolicyDocument = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["Version"] = "2012-10-17",
                ["Statement"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["Action"] = new[]
                        {
                            "secretsmanager:GetSecretValue",
                        },
                        ["Effect"] = "Allow",
                        ["Resource"] = "*",
                    },
                },
            }),
        });


        var rolePolicyAttachmentSecretManager = new RolePolicyAttachment($"{projectName}-LambdaRoleAttachment-SecretManager-{environment}", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{role.Name}"),
            PolicyArn = ManagedPolicy.SecretsManagerReadWrite.ToString(), // TODO: Swap for secretManagerReadPolicy policy if configs correct
        });

        var defaultSecurityGroup = Pulumi.Aws.Ec2.GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
        {
            VpcId = Output.Format($"{stackReferenceVpcId}")
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = defaultSecurityGroup.Apply(o => $"{o.Id}"),
            SubnetIds = privateSubnetIds
        };

        var appArtifactPath = Environment.GetEnvironmentVariable("APPLICATION_ARTIFACT_PATH") ?? config.Require("appArtifactPath");
        Pulumi.Log.Info($"Application Artifact Path: {appArtifactPath}");

        var secretDbConnectionString = new Pulumi.Aws.SecretsManager.Secret($"{projectName}-dbConnectionStringSecret-{environment}");
        var secretVersionDbConnectionString = new Pulumi.Aws.SecretsManager.SecretVersion($"{projectName}-dbConnectionStringSecretVersion-{environment}", new()
        {
            SecretId = secretDbConnectionString.Id,
            SecretString = Output.All(dbConfigs.dbHostName, dbConfigs.dbPassword)
                .Apply(pulumiOutputs => $"Host={pulumiOutputs[0]};Database={config.Require("dbName")};Username={config.Require("dbAdmin")};Password={pulumiOutputs[1]}"),
        });

        DbConnectionStringSecretId = secretDbConnectionString.Name;

        var lambdaFunction = new Function($"{projectName}-{environment}", new FunctionArgs
        {
            Role = role.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 30,
            MemorySize = 1024,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    {
                        "ASPNETCORE_ENVIRONMENT", environment
                    },
                    {
                        "DB_CONNECTION_SECRET_NAME", secretDbConnectionString.Name
                    },
                    {
                        "ExternalSources:ISO3166CountriesURL", Output.Format($"{codesetsEndpointUrl}/resources/ISO3166CountriesURL")
                    },
                    {
                        "ExternalSources:OccupationsEscoURL", Output.Format($"{codesetsEndpointUrl}/resources/OccupationsEscoURL")
                    },
                    {
                        "ExternalSources:OccupationsFlatURL", Output.Format($"{codesetsEndpointUrl}/resources/OccupationsFlatURL")
                    },
                    {
                        "ExternalSources:ISO639Languages", Output.Format($"{codesetsEndpointUrl}/resources/ISO639Languages")
                    },
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs
        });

        var functionUrl = new FunctionUrl($"{projectName}-FunctionUrl-{environment}", new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.Arn,
            AuthorizationType = "NONE"
        });

        var localCommand = new Command($"{projectName}-AddPermissions-{environment}", new CommandArgs
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
        this.DefaultSecurityGroupId = defaultSecurityGroup.Apply(o => $"{o.Id}");
    }

    private (Output<string> dbPassword, Output<string> dbHostName, Output<string> dbSubnetGroupName) InitializePostGresDatabase(Config config, InputMap<string> tags, bool isProductionEnvironment, InputList<string> privateSubNetIds, string environment, string projectName)
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

        var rdsPostGreInstance = new Instance($"{projectName}-postgres-db-{environment}", new InstanceArgs()
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

    public Bucket CreateBucket(InputMap<string> tags, string environment, string projectName)
    {
        var bucket = new Bucket($"{projectName}-{environment}", new BucketArgs()
        {
            Tags = tags,
        });

        return bucket;
    }

    public Output<string> UploadListsData(Bucket bucket, InputMap<string> tags, string objectName, string OutputVariable)
    {
        var bucketObject = new BucketObject(objectName, new BucketObjectArgs
        {
            Bucket = bucket.BucketName,
            Source = new FileAsset($"./Resources/{objectName}"),
            Tags = tags,
            ContentType = "application/json",
            Acl = "public-read"
        });

        var output = Output.Format($"https://{bucket.BucketDomainName}/{objectName}");

        var classProperty = this.GetType().GetProperty(OutputVariable);

        if (classProperty is not null)
        {
            classProperty.SetValue(this, output, null);
        }

        return output;
    }

    [Output] public Output<string> ApplicationUrl { get; set; }
    [Output] public Output<string> CountriesCodeSetUrl { get; set; } = null!;
    [Output] public Output<string> OccupationsCodeSetUrl { get; set; } = null!;
    [Output] public Output<string> OccupationsFlatCodeSetUrl { get; set; } = null!;

    [Output] public Output<ImmutableArray<string>> PrivateSubNetIds { get; set; }

    [Output] public Output<string> VpcId { get; set; }

    [Output] public Output<string> DefaultSecurityGroupId { get; set; }
    [Output] public Output<string> DbPassword { get; set; } = null!;


    [Output] public Output<string> DbConnectionStringSecretId { get; set; }

}
