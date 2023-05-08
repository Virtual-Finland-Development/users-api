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
        VpcId = Output.Format($"{stackReferenceVpcId}");

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
        PrivateSubNetIds = privateSubnetIds;

        var stackSetup = new StackSetup()
        {
            ProjectName = projectName,
            Environment = environment,
            IsProductionEnvironment = isProductionEnvironment,
            Tags = tags,
        };

        var dbConfigs = InitializePostGresDatabase(config, stackSetup);
        DbIdentifier = dbConfigs.DbIdentifier;

        var secretManagerSecret = InitializeSecretManagerForDatabaseCredentials(config, stackSetup, dbConfigs);
        DbConnectionStringSecretId = secretManagerSecret.Name;

        var lambdaFunctionConfigs = InitializeLambdaFunction(config, stackSetup);
        ApplicationUrl = lambdaFunctionConfigs.ApplicationUrl;
        DefaultSecurityGroupId = lambdaFunctionConfigs.DefaultSecurityGroupId;
        LambdaId = lambdaFunctionConfigs.LambdaFunctionArn;
    }

    /// <summary>
    /// Creates the users-api database
    /// </summary>
    private DatabaseSetup InitializePostGresDatabase(Config config, StackSetup stackSetup)
    {
        var dbSubNetGroup = new Pulumi.Aws.Rds.SubnetGroup("dbsubnets", new()
        {
            SubnetIds = PrivateSubNetIds,

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

        this.DbPassword = password.Result;

        return new DatabaseSetup()
        {
            DbHostName = rdsPostGreInstance.Endpoint,
            DbIdentifier = rdsPostGreInstance.Identifier,
            DbPassword = password.Result,
        };
    }

    /// <summary>
    /// Creates the users-api database credentials in AWS Secrets Manager
    /// </summary>
    private Pulumi.Aws.SecretsManager.Secret InitializeSecretManagerForDatabaseCredentials(Config config, StackSetup stackSetup, DatabaseSetup dbConfigs)
    {
        var secretDbConnectionString = new Pulumi.Aws.SecretsManager.Secret($"{stackSetup.ProjectName}-dbConnectionStringSecret-{stackSetup.Environment}");
        new Pulumi.Aws.SecretsManager.SecretVersion($"{stackSetup.ProjectName}-dbConnectionStringSecretVersion-{stackSetup.Environment}", new()
        {
            SecretId = secretDbConnectionString.Id,
            SecretString = Output.All(dbConfigs.DbHostName, dbConfigs.DbPassword)
                .Apply(pulumiOutputs => $"Host={pulumiOutputs[0]};Database={config.Require("dbName")};Username={config.Require("dbAdmin")};Password={pulumiOutputs[1]}"),
        });

        return secretDbConnectionString;
    }


    /// <summary>
    /// Creates the users-api lambda function and related resources
    /// </summary>
    private LambdaFunctionSetup InitializeLambdaFunction(Config config, StackSetup stackSetup)
    {
        // External references
        var codeSetStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/codesets/{stackSetup.Environment}");
        var codesetsEndpointUrl = codeSetStackReference.GetOutput("url");

        // Lambda function
        var execRole = new Role($"{stackSetup.ProjectName}-LambdaRole-{stackSetup.Environment}", new RoleArgs
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

        var rolePolicyAttachment = new RolePolicyAttachment($"{stackSetup.ProjectName}-LambdaRoleAttachment-{stackSetup.Environment}", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        var secretManagerReadPolicy = new Pulumi.Aws.Iam.Policy($"{stackSetup.ProjectName}-LambdaSecretManagerPolicy-{stackSetup.Environment}", new()
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


        var rolePolicyAttachmentSecretManager = new RolePolicyAttachment($"{stackSetup.ProjectName}-LambdaRoleAttachment-SecretManager-{stackSetup.Environment}", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.SecretsManagerReadWrite.ToString(), // TODO: Swap for secretManagerReadPolicy policy if configs correct
        });

        var defaultSecurityGroup = Pulumi.Aws.Ec2.GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
        {
            VpcId = VpcId
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = defaultSecurityGroup.Apply(o => $"{o.Id}"),
            SubnetIds = PrivateSubNetIds
        };

        var appArtifactPath = Environment.GetEnvironmentVariable("APPLICATION_ARTIFACT_PATH") ?? config.Require("appArtifactPath");
        Pulumi.Log.Info($"Application Artifact Path: {appArtifactPath}");

        var lambdaFunction = new Function($"{stackSetup.ProjectName}-{stackSetup.Environment}", new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 30,
            MemorySize = 1024,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    {
                        "ASPNETCORE_ENVIRONMENT", stackSetup.Environment
                    },
                    {
                        "DB_CONNECTION_SECRET_NAME", DbConnectionStringSecretId
                    },
                    {
                        "CodesetApiBaseUrl", Output.Format($"{codesetsEndpointUrl}/resources")
                    },
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs
        });

        var functionUrl = new FunctionUrl($"{stackSetup.ProjectName}-FunctionUrl-{stackSetup.Environment}", new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.Arn,
            AuthorizationType = "NONE"
        });

        new Command($"{stackSetup.ProjectName}-AddPermissions-{stackSetup.Environment}", new CommandArgs
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

        return new LambdaFunctionSetup()
        {
            ApplicationUrl = functionUrl.FunctionUrlResult,
            LambdaFunctionArn = lambdaFunction.Arn,
            DefaultSecurityGroupId = defaultSecurityGroup.Apply(o => $"{o.Id}"),
        };
    }

    public record StackSetup
    {
        public InputMap<string> Tags = default!;
        public bool IsProductionEnvironment;
        public string Environment = default!;
        public string ProjectName = default!;
    }

    public record DatabaseSetup
    {
        public Output<string> DbPassword = default!;
        public Output<string> DbHostName = default!;
        public Output<string> DbSubnetGroupName = default!;
        public Output<string> DbIdentifier = default!;
    }

    public record LambdaFunctionSetup
    {
        public Output<string> ApplicationUrl = default!;
        public Output<string> DefaultSecurityGroupId = default!;
        public Output<string> LambdaFunctionArn = default!;
    }

    // Outputs for Pulumi service
    [Output] public Output<string> ApplicationUrl { get; set; }
    [Output] public Output<IEnumerable<string>> PrivateSubNetIds { get; set; }
    [Output] public Output<string> VpcId { get; set; }
    [Output] public Output<string> DefaultSecurityGroupId { get; set; }
    [Output] public Output<string> DbPassword { get; set; } = null!;
    [Output] public Output<string> DbConnectionStringSecretId { get; set; }
    [Output] public Output<string> DbIdentifier { get; set; }
    [Output] public Output<string> LambdaId { get; set; }
}
