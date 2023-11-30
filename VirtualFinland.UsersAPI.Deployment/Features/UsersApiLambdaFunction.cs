using System;
using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Aws.Sqs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api lambda function and related resources
/// </summary>
class UsersApiLambdaFunction
{
    public UsersApiLambdaFunction(Config config, StackSetup stackSetup, VpcSetup vpcSetup, SecretsManager secretsManager, RedisElastiCache redis, CloudWatch cloudwatch, Queue analyticsSqS, PostgresDatabase database)
    {
        // External references
        var codesetStackReference = new StackReference($"{Pulumi.Deployment.Instance.OrganizationName}/codesets/{stackSetup.Environment}");
        var codesetsEndpointUrl = codesetStackReference.GetOutput("url");

        // Retrieve ACL configs
        var stackReference = new StackReference(stackSetup.GetInfrastructureStackName());
        var sharedAccessKey = stackReference.RequireOutput("SharedAccessKey");
        var aclConfig = new Config("acl");
        var authorizationConfig = new Config("auth");
        var termsOfServiceConfig = new Config("termsOfService");

        // Lambda function
        var execRole = new Role(stackSetup.CreateResourceName("LambdaRole"), new RoleArgs
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

        var rolePolicyAttachment = new RolePolicyAttachment(stackSetup.CreateResourceName("LambdaRoleAttachment"), new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{execRole.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaVPCAccessExecutionRole.ToString()
        });

        var secretsManagerReadPolicy = new Policy(stackSetup.CreateResourceName("LambdaSecretManagerPolicy"), new()
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

        // Grant access to the secret manager
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("LambdaRoleAttachment-SecretManager"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = secretsManagerReadPolicy.Arn
        });

        // Allow function to access elasticache
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("LambdaRoleAttachment-ElastiCache"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = ManagedPolicy.AmazonElastiCacheFullAccess.ToString()
        });

        // Allow function to post metrics to cloudwatch
        var cloudWatchMetricsPushPolicy = new Policy(stackSetup.CreateResourceName("LambdaCloudWatchMetricsPushPolicy"), new()
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
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("LambdaRoleAttachment-CloudWatch"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = cloudWatchMetricsPushPolicy.Arn
        });

        // Allow function to send SQS messages
        var sqsSendMessagePolicy = new Policy(stackSetup.CreateResourceName("LambdaSQSSendMessagePolicy"), new()
        {
            Description = "Users-API SQS Send Message Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""sqs:SendMessage""
                        ],
                        ""Resource"": [
                            ""{analyticsSqS.Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });
        _ = new RolePolicyAttachment(stackSetup.CreateResourceName("LambdaRoleAttachment-SQS"), new RolePolicyAttachmentArgs
        {
            Role = execRole.Name,
            PolicyArn = sqsSendMessagePolicy.Arn
        });

        var defaultSecurityGroup = GetSecurityGroup.Invoke(new GetSecurityGroupInvokeArgs()
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
        LambdaFunctionResource = new Function(stackSetup.CreateResourceName("LambdaFunction"), new FunctionArgs
        {
            Role = execRole.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 15,
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
                        "REDIS_ENDPOINT", Output.Format($"{redis.ClusterEndpoint}")
                    },
                    {
                        "Services__Codesets__ApiEndpoint", Output.Format($"{codesetsEndpointUrl}/resources")
                    },
                    {
                        "Security__Access__AccessFinland__IsEnabled", aclConfig.Require("accessfinland-isEnabled")
                    },
                    {
                        "Security__Access__AccessFinland__AccessKeys__0", Output.Format($"{sharedAccessKey}")
                    },
                    {
                        "Security__Access__Dataspace__IsEnabled", aclConfig.Require("dataspace-isEnabled")
                    },
                    {
                        "Security__Access__Dataspace__AccessKeys__0", aclConfig.Require("dataspace-agent") // Note: maybe override the appsettings list values some other way?
                    },
                    {
                        "Security__Access__Dataspace__AccessKeys__1", "" // Overwrite the default value
                    },
                    {
                        "Security__Authorization__Testbed__IsEnabled", authorizationConfig.Require("testbed-isEnabled")
                    },
                    {
                        "Security__Authorization__Sinuna__IsEnabled", authorizationConfig.Require("sinuna-isEnabled")
                    },
                    {
                        "Security__Options__TermsOfServiceAgreementRequired", termsOfServiceConfig.Require("isEnabled")
                    },
                    {
                        "Analytics__CloudWatch__IsEnabled", "true"
                    },
                    {
                        "Analytics__SQS__QueueUrl", analyticsSqS.Url
                    },
                    {
                        "Analytics__SQS__IsEnabled", "true"
                    }
                }
            },
            Code = new FileArchive(appArtifactPath),
            VpcConfig = functionVpcArgs,
            Tags = stackSetup.Tags
        }, new() { DependsOn = new[] { database.MainResource } });

        // Configure log group with retention of 180 days
        LogGroup = cloudwatch.CreateLambdaFunctionLogGroup(stackSetup, "apiFunction", LambdaFunctionResource, 180);

        LambdaFunctionArn = LambdaFunctionResource.Arn;
        LambdaFunctionId = LambdaFunctionResource.Id;
    }

    public void SetupErrorAlerting(StackSetup stackSetup)
    {
        var stackReference = new StackReference(stackSetup.GetAlertingStackName());
        var errorLambdaFunctionArnRef = stackReference.RequireOutput("errorSubLambdaFunctionArn");
        var errorLambdaFunctionArn = Output.Format($"{errorLambdaFunctionArnRef}");

        // Permissions for the log group subscription to invoke the error alerting lambda function of monitoring stack
        var logGroupInvokePermission = new Permission(stackSetup.CreateResourceName("ErrorAlerter"), new PermissionArgs
        {
            Action = "lambda:InvokeFunction",
            Function = errorLambdaFunctionArn,
            Principal = "logs.amazonaws.com",
            SourceArn = Output.Format($"{LogGroup.Arn}:*"),
        }, new() { DependsOn = { LogGroup } });

        // Subscribe to the log groups
        _ = new LogSubscriptionFilter(stackSetup.CreateResourceName("StatusCodeAlert"), new LogSubscriptionFilterArgs
        {
            LogGroup = LogGroup.Name,
            FilterPattern = "{ $.StatusCode > 404 || $.StatusCode = 400 }", // Users API should not encounter errors with status code > 404, for example validation errors not expected. 400 is also considered an error.
            DestinationArn = errorLambdaFunctionArn,
        }, new() { DependsOn = { logGroupInvokePermission } });

        _ = new LogSubscriptionFilter(stackSetup.CreateResourceName("TaskTimedOutAlert"), new LogSubscriptionFilterArgs
        {
            LogGroup = LogGroup.Name,
            FilterPattern = "%Task timed out%", // Users API should not encounter timeouts
            DestinationArn = errorLambdaFunctionArn,
        }, new() { DependsOn = { logGroupInvokePermission } });
    }

    public Function LambdaFunctionResource = default!;
    public Output<string> LambdaFunctionArn = default!;
    public Output<string> LambdaFunctionId = default!;
    public LogGroup LogGroup = default!;
}