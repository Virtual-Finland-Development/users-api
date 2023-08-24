using System;
using Pulumi;
using Pulumi.Aws.WafV2;
using Pulumi.Aws.WafV2.Inputs;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class WebApplicationFirewall
{
    public WebApplicationFirewall(StackSetup stackSetup, ApiGatewayForLambdaFunction apiGw)
    {
        // Retrieve stack reference from infrastructure stack
        var stackReference = new StackReference(stackSetup.GetInfrastructureStackName());
        var sharedAccessKey = stackReference.RequireOutput("SharedAccessKey");

        // Acl config
        var aclConfig = new Config("acl");

        // Create web acl v2 for the api gateway
        var webAcl = new WebAcl($"{stackSetup.ProjectName}-WebAcl-{stackSetup.Environment}", new()
        {
            Description = "Web ACL for the api gateway",
            Scope = "REGIONAL",
            Tags = stackSetup.Tags,
            VisibilityConfig = new WebAclVisibilityConfigArgs
            {
                CloudwatchMetricsEnabled = true,
                MetricName = "WebAcl",
                SampledRequestsEnabled = true
            },
            DefaultAction = new WebAclDefaultActionArgs
            {
                Block = new WebAclDefaultActionBlockArgs()
                {
                    CustomResponse = new WebAclDefaultActionBlockCustomResponseArgs
                    {
                        ResponseCode = 403,
                        CustomResponseBodyKey = "AccessDenied"
                    }
                }
            },
            CustomResponseBodies = new InputList<WebAclCustomResponseBodyArgs>
            {
                new WebAclCustomResponseBodyArgs
                {
                    Key = "AccessDenied",
                    ContentType = "application/json",
                    Content = "{\"message\":\"Access denied\"}",
                }
            },
            Rules = new InputList<WebAclRuleArgs>
            {
                new WebAclRuleArgs
                {
                    Name = "GrantAccessToHealthCheckPath",
                    Priority = 5,
                    Action = new WebAclRuleActionArgs
                    {
                        Allow = new WebAclRuleActionAllowArgs()
                    },
                    Statement = new WebAclRuleStatementArgs
                    {
                        ByteMatchStatement = new WebAclRuleStatementByteMatchStatementArgs
                        {
                            FieldToMatch = new WebAclRuleStatementByteMatchStatementFieldToMatchArgs
                            {
                                UriPath = new WebAclRuleStatementByteMatchStatementFieldToMatchUriPathArgs()
                            },
                            TextTransformations = new InputList<WebAclRuleStatementByteMatchStatementTextTransformationArgs>
                            {
                                new WebAclRuleStatementByteMatchStatementTextTransformationArgs
                                {
                                    Priority = 0,
                                    Type = "NONE"
                                }
                            },
                            PositionalConstraint = "EXACTLY",
                            SearchString = "/"
                        }
                    },
                    VisibilityConfig = new WebAclRuleVisibilityConfigArgs
                    {
                        CloudwatchMetricsEnabled = false,
                        MetricName = "GrantAccessToHealthCheckPath",
                        SampledRequestsEnabled = false
                    }
                },
                new WebAclRuleArgs
                {
                    Name = "GrantAccessToDataspaceRequests",
                    Priority = 4,
                    Action = new WebAclRuleActionArgs
                    {
                        Allow = new WebAclRuleActionAllowArgs()
                    },
                    Statement = new WebAclRuleStatementArgs
                    {
                        ByteMatchStatement = new WebAclRuleStatementByteMatchStatementArgs
                        {
                            FieldToMatch = new WebAclRuleStatementByteMatchStatementFieldToMatchArgs
                            {
                                SingleHeader = new WebAclRuleStatementByteMatchStatementFieldToMatchSingleHeaderArgs
                                {
                                    Name = "User-Agent"
                                }
                            },
                            TextTransformations = new InputList<WebAclRuleStatementByteMatchStatementTextTransformationArgs>
                            {
                                new WebAclRuleStatementByteMatchStatementTextTransformationArgs
                                {
                                    Priority = 0,
                                    Type = "NONE"
                                }
                            },
                            PositionalConstraint = "EXACTLY",
                            SearchString = aclConfig.Require("dataspaceAgent")
                        }
                    },
                    VisibilityConfig = new WebAclRuleVisibilityConfigArgs
                    {
                        CloudwatchMetricsEnabled = true,
                        MetricName = "GrantAccessToDataspaceRequests",
                        SampledRequestsEnabled = true
                    }
                },
                new WebAclRuleArgs
                {
                    Name = "GrantAccessFromAccessFinlandBackend",
                    Priority = 0,
                    Action = new WebAclRuleActionArgs
                    {
                        Allow = new WebAclRuleActionAllowArgs()
                    },
                    Statement = new WebAclRuleStatementArgs
                    {
                        AndStatement = new WebAclRuleStatementAndStatementArgs
                        {
                            Statements = new InputList<WebAclRuleStatementAndStatementStatementArgs>
                                {
                                    new WebAclRuleStatementAndStatementStatementArgs
                                    {
                                        ByteMatchStatement = new WebAclRuleStatementAndStatementStatementByteMatchStatementArgs
                                        {
                                            FieldToMatch = new WebAclRuleStatementAndStatementStatementByteMatchStatementFieldToMatchArgs
                                            {
                                                SingleHeader = new WebAclRuleStatementAndStatementStatementByteMatchStatementFieldToMatchSingleHeaderArgs
                                                {
                                                    Name = "X-Api-Key"
                                                }
                                            },
                                            TextTransformations = new InputList<WebAclRuleStatementAndStatementStatementByteMatchStatementTextTransformationArgs>
                                            {
                                                new WebAclRuleStatementAndStatementStatementByteMatchStatementTextTransformationArgs
                                                {
                                                    Priority = 0,
                                                    Type = "NONE"
                                                }
                                            },
                                            PositionalConstraint = "EXACTLY",
                                            SearchString = sharedAccessKey.Apply(x => x.ToString() ?? throw new ArgumentNullException(nameof(sharedAccessKey)))
                                        }
                                    },
                                    /* new WebAclRuleStatementAndStatementStatementArgs
                                    {
                                        IpSetReferenceStatement = new WebAclRuleStatementAndStatementStatementIpSetReferenceStatementArgs
                                        {
                                            Arn = stackSetup.IpSetArn
                                        }
                                    } */
                                }
                        }
                    },
                    VisibilityConfig = new WebAclRuleVisibilityConfigArgs
                    {
                        CloudwatchMetricsEnabled = true,
                        MetricName = "GrantAccessFromAccessFinlandBackend",
                        SampledRequestsEnabled = true
                    }
                },
            },
        });


        // Create web acl association
        new WebAclAssociation($"{stackSetup.ProjectName}-WebAclAssociation-{stackSetup.Environment}", new()
        {
            ResourceArn = apiGw.ApiGatewayV2Resource.Arn,
            WebAclArn = webAcl.Arn
        });
    }
}