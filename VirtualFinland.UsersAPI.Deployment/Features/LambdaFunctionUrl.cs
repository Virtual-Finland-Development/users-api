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
class LambdaFunctionUrl
{
    public LambdaFunctionUrl(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager)
    {
        // External references
        var codesetStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/codesets/{stackSetup.Environment}");
        var codesetsEndpointUrl = codesetStackReference.GetOutput("url");

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
        Pulumi.Log.Info($"Application Artifact Path: {appArtifactPath}");

        var lambdaFunction = new Function($"{stackSetup.ProjectName}-{stackSetup.Environment}", new FunctionArgs
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
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
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

        ApplicationUrl = functionUrl.FunctionUrlResult;
        LambdaFunctionArn = lambdaFunction.Arn;
        LambdaFunctionId = lambdaFunction.Id;
    }

    public Output<string> ApplicationUrl = default!;
    public Output<string> LambdaFunctionArn = default!;
    public Output<string> LambdaFunctionId = default!;
}