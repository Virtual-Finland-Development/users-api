using System;
using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class DatabaseMigratorLambda
{
    public DatabaseMigratorLambda(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager)
    {
        // Lambda function
        var execRole = new Role(stackSetup.CreateResourceName("DatabaseMigratorLambdaRole"), new RoleArgs
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
            }),
            Tags = stackSetup.Tags
        });

        new RolePolicyAttachment(stackSetup.CreateResourceName("DatabaseMigratorLambdaRoleAttachment"), new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        new RolePolicyAttachment(stackSetup.CreateResourceName("DatabaseMigratorLambdaRoleAttachment-SecretManager"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = secretsManager.ReadPolicy.Arn
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = new[] { vpcSetup.SecurityGroupId },
            SubnetIds = vpcSetup.PrivateSubnetIds
        };

        var appArtifactPath = Environment.GetEnvironmentVariable("DB_MIGRATOR_ARTIFACT_PATH") ?? config.Require("dbMigratorArtifactPath");
        var lambdaFunction = new Function(stackSetup.CreateResourceName("AdminFunction"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::FunctionHandler",
            Timeout = 30,
            MemorySize = 256,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    {
                        "ASPNETCORE_ENVIRONMENT", stackSetup.Environment
                    },
                    {
                        "DB_CONNECTION_SECRET_NAME", secretsManager.Name
                    },
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        });

        LambdaFunctionArn = lambdaFunction.Arn;
        LambdaFunctionId = lambdaFunction.Id;
    }

    public Output<string> LambdaFunctionArn = default!;
    public Output<string> LambdaFunctionId = default!;
}