using System;
using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class AuditLogSubscription
{
    public AuditLogSubscription(Config config, StackSetup stackSetup, PostgresDatabase postgresDatabase, CloudWatch cloudwatch)
    {
        // Lambda function
        var execRole = new Role(stackSetup.CreateResourceName("AuditLogSubscriptionRole"), new RoleArgs
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

        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AuditLogSubscriptionRoleAttachment"), new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });


        var appArtifactPath = Environment.GetEnvironmentVariable("AUDITLOG_SUBSCRIPTION_ARTIFACT_PATH") ?? config.Require("auditlogSubscriptionArtifactPath");
        var lambdaFunction = new Function(stackSetup.CreateResourceName("AuditLogSubscription"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AuditLogSubscription::VirtualFinland.AuditLogSubscription.Function::FunctionHandler",
            Timeout = 6,
            MemorySize = 128,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    {
                        "ASPNETCORE_ENVIRONMENT", stackSetup.Environment
                    },
                }
            },
            Code = new FileArchive(appArtifactPath),
            Tags = stackSetup.Tags
        });

        // Permissions for the log group to invoke the auditlog parsing lambda function
        var logGroupInvokePermission = new Permission(stackSetup.CreateResourceName("AuditLogSubscriptionInvokePermission"), new PermissionArgs
        {
            Action = "lambda:InvokeFunction",
            Function = lambdaFunction.Name,
            Principal = "logs.amazonaws.com",
            SourceArn = Output.Format($"{postgresDatabase.LogGroup.Arn}:*"),
        }, new() { DependsOn = { lambdaFunction } });

        // Subsrcibe to the log group
        _ = new LogSubscriptionFilter(stackSetup.CreateResourceName("AuditLogSubscriptionFilter"), new LogSubscriptionFilterArgs
        {
            LogGroup = postgresDatabase.LogGroup.Name,
            FilterPattern = "%AuditLog%",
            DestinationArn = lambdaFunction.Arn,
        }, new() { DependsOn = { logGroupInvokePermission } });

        LambdaFunctionArn = lambdaFunction.Arn;
        LambdaFunctionId = lambdaFunction.Id;
    }

    public Output<string> LambdaFunctionArn = default!;
    public Output<string> LambdaFunctionId = default!;
}