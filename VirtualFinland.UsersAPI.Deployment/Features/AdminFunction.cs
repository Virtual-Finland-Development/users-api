using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class AdminFunction
{
    public AdminFunction(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager, Queue analyticsSqS, PostgresDatabase database)
    {
        // Lambda function
        var execRole = new Role(stackSetup.CreateResourceName("AdminFunctionRole"), new RoleArgs
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

        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AdminFunctionRoleAttachment"), new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AdminFunctionRoleAttachment-SecretManager"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = secretsManager.ReadPolicy.Arn
        });

        // Allow function to post metrics to cloudwatch
        var cloudWatchMetricsPushPolicy = new Policy(stackSetup.CreateResourceName("AdminFunctionCloudWatchMetricsPushPolicy"), new()
        {
            Description = "Users-API CloudWatch Metrics Push Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""cloudwatch:PutMetricData""
                        ],
                        ""Resource"": [
                            ""*""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AdminFunctionRoleAttachment-CloudWatch"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = cloudWatchMetricsPushPolicy.Arn
        });

        // Allow function to be invoked by SQS
        var sqsInvokePolicy = new Policy(stackSetup.CreateResourceName("AdminFunctionSQSInvokePolicy"), new()
        {
            Description = "Users-API SQS Invoke Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""sqs:ReceiveMessage"",
                            ""sqs:DeleteMessage"",
                            ""sqs:GetQueueAttributes""
                        ],
                        ""Resource"": [
                            ""{analyticsSqS.Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AdminFunctionRoleAttachment-SQS"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = sqsInvokePolicy.Arn
        });

        var functionVpcArgs = new FunctionVpcConfigArgs()
        {
            SecurityGroupIds = new[] { vpcSetup.SecurityGroupId },
            SubnetIds = vpcSetup.PrivateSubnetIds
        };

        var appArtifactPath = config.Require("adminFunctionArtifactPath");
        var environmentArg = new FunctionEnvironmentArgs
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
                        "Analytics__CloudWatch__IsEnabled", "true"
                    },
                    {
                        "Database__Triggers__SQS__IsEnabled", "false" // Ensure no SQS Triggers are enabled
                    }
                }
        };

        LambdaFunction = new Function(stackSetup.CreateResourceName("AdminFunction"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::FunctionHandler",
            Timeout = 30,
            MemorySize = 256,
            Environment = environmentArg,
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });

        SqsEventHandlerFunction = new Function(stackSetup.CreateResourceName("AdminFunction-sqs"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::SQSEventHandler",
            Timeout = 30,
            MemorySize = 128, // Intented for short-lived, low memory tasks
            Environment = environmentArg,
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });

        CloudWatchEventHandlerFunction = new Function(stackSetup.CreateResourceName("AdminFunction-cloudwatch"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::FunctionHandler",
            Timeout = 30,
            MemorySize = 256,
            Environment = environmentArg,
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });
    }

    public void CreateSchedulersAndTriggers(StackSetup stackSetup, Queue analyticsSqS)
    {
        CreateAnalyticsUpdateTriggers(stackSetup, analyticsSqS);
        CreateAnalyticsUpdateScheduler(stackSetup);
        CreateCleanupSchedulers(stackSetup);
    }

    public void CreateAnalyticsUpdateTriggers(StackSetup stackSetup, Queue analyticsSqS)
    {
        // Configure SQS trigger
        _ = new EventSourceMapping(stackSetup.CreateResourceName("AdminFunctionSQSTrigger"), new EventSourceMappingArgs
        {
            EventSourceArn = analyticsSqS.Arn,
            FunctionName = SqsEventHandlerFunction.Name,
            BatchSize = 1,
            Enabled = true,
        });
    }

    public void CreateAnalyticsUpdateScheduler(StackSetup stackSetup)
    {
        // Configure CloudWatch scheduled event
        var eventRule = new EventRule(stackSetup.CreateResourceName("analytics-update-scheduler"), new EventRuleArgs
        {
            ScheduleExpression = "rate(3 hours)",
            Description = "Users-API Analytics Update Trigger",
            Tags = stackSetup.Tags
        });
        _ = new Permission(stackSetup.CreateResourceName("analytics-update-scheduler-permission"), new PermissionArgs
        {
            Principal = "events.amazonaws.com",
            Action = "lambda:InvokeFunction",
            Function = CloudWatchEventHandlerFunction.Name,
            SourceArn = eventRule.Arn
        });
        _ = new EventTarget(stackSetup.CreateResourceName("analytics-update-scheduler-target"), new EventTargetArgs
        {
            Rule = eventRule.Name,
            Arn = CloudWatchEventHandlerFunction.Arn,
            Input = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                { "Action", "UpdateAnalytics" }
            })
        });
    }

    public void CreateCleanupSchedulers(StackSetup stackSetup)
    {
        // Configure CloudWatch scheduled event
        var eventRule = new EventRule(stackSetup.CreateResourceName("cleanups-scheduler"), new EventRuleArgs
        {
            ScheduleExpression = "rate(1 days)",
            Description = "Users-API Cleanup Trigger",
            Tags = stackSetup.Tags
        });
        _ = new Permission(stackSetup.CreateResourceName("cleanups-scheduler-permission"), new PermissionArgs
        {
            Principal = "events.amazonaws.com",
            Action = "lambda:InvokeFunction",
            Function = CloudWatchEventHandlerFunction.Name,
            SourceArn = eventRule.Arn
        });
        _ = new EventTarget(stackSetup.CreateResourceName("cleanups-scheduler-target"), new EventTargetArgs
        {
            Rule = eventRule.Name,
            Arn = CloudWatchEventHandlerFunction.Arn,
            Input = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                { "Action", "RunCleanups" }
            })
        });
    }

    public Function LambdaFunction = default!;
    public Function SqsEventHandlerFunction = default!;
    public Function CloudWatchEventHandlerFunction = default!;
}