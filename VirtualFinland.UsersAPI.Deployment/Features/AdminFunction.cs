using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using static VirtualFinland.UsersAPI.Deployment.Features.SqsQueue;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class AdminFunction
{
    public AdminFunction(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager, Queues adminFunctionSqs, PostgresDatabase database)
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
                            ""{adminFunctionSqs.Fast.Arn}"",
                            ""{adminFunctionSqs.Slow.Arn}""
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

        // Allow sending emails with SES
        var sesSendEmailPolicy = new Policy(stackSetup.CreateResourceName("AdminFunctionSESSendEmailPolicy"), new()
        {
            Description = "Users-API SES Send Email Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""ses:SendEmail"",
                            ""ses:SendRawEmail""
                        ],
                        ""Resource"": [
                            ""*""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("AdminFunctionRoleAttachment-SES"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = sesSendEmailPolicy.Arn
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
                        "Dispatches__SQS__QueueUrls__Fast", ""
                    },
                    {
                        "Dispatches__SQS__QueueUrls__Slow", adminFunctionSqs.Slow.Url
                    },
                    {
                        "Dispatches__SQS__IsEnabled", "true"
                    },
                    {
                        "Notifications__Email__FromAddress", new Config("ses").Require("fromAddress")
                    },
                    {
                        "Notifications__Email__SiteUrl", new Config("ses").Require("siteUrl")
                    }
                }
        };

        LambdaFunction = new Function(stackSetup.CreateResourceName("AdminFunction"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::FunctionHandler",
            Timeout = 120,
            MemorySize = 256,
            Environment = environmentArg,
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });

        SqsFastEventHandlerFunction = new Function(stackSetup.CreateResourceName("AdminFunction-sqs-fast"), new FunctionArgs
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

        SqsSlowEventHandlerFunction = new Function(stackSetup.CreateResourceName("AdminFunction-sqs-slow"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "AdminFunction::VirtualFinland.AdminFunction.Function::SQSEventHandler",
            Timeout = 30,
            MemorySize = 256,
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
            Timeout = 120,
            MemorySize = 256,
            Environment = environmentArg,
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });
    }

    public void CreateSchedulersAndTriggers(StackSetup stackSetup, Queues adminFunctionSqs)
    {
        CreateAdminFunctionQueueTriggers(stackSetup, adminFunctionSqs);
        CreateAnalyticsUpdateScheduler(stackSetup);
        CreateCleanupSchedulers(stackSetup);
    }

    private void CreateAdminFunctionQueueTriggers(StackSetup stackSetup, Queues adminFunctionSqs)
    {
        // Configure SQS trigger for fast track
        _ = new EventSourceMapping(stackSetup.CreateResourceName("AdminFunctionSQSTrigger"), new EventSourceMappingArgs
        {
            EventSourceArn = adminFunctionSqs.Fast.Arn,
            FunctionName = SqsFastEventHandlerFunction.Name,
            BatchSize = 1,
            Enabled = true,
        });

        // Slow
        _ = new EventSourceMapping(stackSetup.CreateResourceName("AdminFunctionSQSTrigger-Slow"), new EventSourceMappingArgs
        {
            EventSourceArn = adminFunctionSqs.Slow.Arn,
            FunctionName = SqsSlowEventHandlerFunction.Name,
            BatchSize = 1,
            Enabled = true,
            MaximumBatchingWindowInSeconds = 60
        });
    }

    private void CreateAnalyticsUpdateScheduler(StackSetup stackSetup)
    {
        // Configure CloudWatch scheduled event
        var eventRule = new EventRule(stackSetup.CreateResourceName("analytics-update-scheduler"), new EventRuleArgs
        {
            ScheduleExpression = "rate(12 hours)",
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

    private void CreateCleanupSchedulers(StackSetup stackSetup)
    {
        // Configure CloudWatch scheduled event
        var eventRule = new EventRule(stackSetup.CreateResourceName("cleanups-scheduler"), new EventRuleArgs
        {
            ScheduleExpression = "rate(1 day)",
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
    public Function SqsFastEventHandlerFunction = default!;
    public Function SqsSlowEventHandlerFunction = default!;
    public Function CloudWatchEventHandlerFunction = default!;
}