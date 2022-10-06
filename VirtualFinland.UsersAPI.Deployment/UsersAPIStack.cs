using System.Collections.Generic;
using System.Text.Json;
using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using Pulumi.Command.Local;

namespace VirtualFinland.UsersAPI.Deployment;

public class UsersAPIStack : Stack
{
    public UsersAPIStack()
    {
        var role = new Role("vf-UsersAPI-LambdaRole", new RoleArgs
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

        var rolePolicyAttachment = new RolePolicyAttachment("vf-UsersAPI-LambdaRoleAttachment", new RolePolicyAttachmentArgs
        {
            Role = Output.Format($"{role.Name}"),
            PolicyArn = ManagedPolicy.AWSLambdaBasicExecutionRole.ToString()
        });

        var lambdaFunction = new Function("vf-UsersAPI", new FunctionArgs
        {
            Role = role.Arn,
            Runtime = "dotnet6",
            Handler = "VirtualFinland.UsersAPI",
            Timeout = 30,
            Environment = new FunctionEnvironmentArgs
            {
                Variables =
                {
                    { "ASPNETCORE_ENVIRONMENT", "Development" }
                }
            },
            Code = new FileArchive("../VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI/bin/Release/net6.0/VirtualFinland.UsersAPI.zip")
        });

        var functionUrl = new FunctionUrl("vf-UsersAPI-FunctionUrl", new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.Arn,
            AuthorizationType = "NONE"
        });

        var localCommand = new Command("vf-UsersAPI-AddPermissions", new CommandArgs
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


        Url = functionUrl.FunctionUrlResult;
    }

    [Output] public Output<string> Url { get; set; }
    }