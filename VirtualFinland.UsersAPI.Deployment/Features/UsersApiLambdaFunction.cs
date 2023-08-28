using System;
using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Command.Local;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api lambda function and related resources
/// </summary>
class UsersApiLambdaFunction
{
    public UsersApiLambdaFunction(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager)
    {
        // External references
        var codesetStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/codesets/{stackSetup.Environment}");
        var codesetsEndpointUrl = codesetStackReference.GetOutput("url");

        // Retrieve ACL configs
        var stackReference = new StackReference(stackSetup.GetInfrastructureStackName());
        var sharedAccessKey = stackReference.RequireOutput("SharedAccessKey");
        var aclConfig = new Config("acl");

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
            }),
            Tags = stackSetup.Tags
        });

        var rolePolicyAttachment = new RolePolicyAttachment($"{stackSetup.ProjectName}-LambdaRoleAttachment-{stackSetup.Environment}", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        var secretsManagerReadPolicy = new Policy($"{stackSetup.ProjectName}-LambdaSecretManagerPolicy-{stackSetup.Environment}", new()
        {
            Description = "Users-API Secret Get Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""secretsmanager:GetSecretValue"",
                            ""secretsmanager:DescribeSecret""
                        ],
                        ""Resource"": [
                            ""{secretsManager.Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });

        new RolePolicyAttachment($"{stackSetup.ProjectName}-LambdaRoleAttachment-SecretManager-{stackSetup.Environment}", new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = secretsManagerReadPolicy.Arn
        });

        var defaultSecurityGroup = Pulumi.Aws.Ec2.GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
        {
            VpcId = vpcSetup.VpcId,
            Name = "default"
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = defaultSecurityGroup.Apply(o => $"{o.Id}"),
            SubnetIds = vpcSetup.PrivateSubnetIds
        };

        var appArtifactPath = Environment.GetEnvironmentVariable("APPLICATION_ARTIFACT_PATH") ?? config.Require("appArtifactPath");
        LambdaFunctionResource = new Function($"{stackSetup.ProjectName}-{stackSetup.Environment}", new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 30,
            MemorySize = 2048,
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
                    {
                        "CodesetApiBaseUrl", Output.Format($"{codesetsEndpointUrl}/resources")
                    },
                    {
                        "Security__Access__AccessFinland__AccessKeys__0", Output.Format($"{sharedAccessKey}") // Note: maybe override the appsettings list values some other way?
                    },
                    {
                        "Security__Access__AccessFinland__IsEnabled", aclConfig.Require("accessfinlandIsEnabled")
                    },
                    {
                        "Security__Access__Dataspace__AccessKeys__0", aclConfig.Require("dataspaceAagent")
                    },
                    {
                        "Security__Access__Dataspace__AccessKeys__1", "" // Overwrite the default value
                    },
                    {
                        "Security__Access__Dataspace__IsEnabled", aclConfig.Require("dataspaceIsEnabled")
                    },
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        });

        LambdaFunctionArn = LambdaFunctionResource.Arn;
        LambdaFunctionId = LambdaFunctionResource.Id;
    }

    public Function LambdaFunctionResource = default!;
    public Output<string> LambdaFunctionArn = default!;
    public Output<string> LambdaFunctionId = default!;
}