
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Lambda;

using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class CloudWatch
{
    private readonly StackSetup _stackSetup;
    public CloudWatch(StackSetup stackSetup) => _stackSetup = stackSetup;

    public LogGroup CreateLambdaFunctionLogGroup(StackSetup stackSetup, string name, Function lambdaFunction, int retentionInDays = 30)
    {
        // Configure log retention for new installations
        return CreateLogGroup(stackSetup, name, Output.Format($"/aws/lambda/{lambdaFunction.Name}"), retentionInDays);
    }

    public LogGroup CreateLogGroup(StackSetup stackSetup, string name, Output<string> logGroupIdentifier, int retentionInDays = 30)
    {
        // Configure log retention for new installations
        return new LogGroup(stackSetup.CreateResourceName(name), new LogGroupArgs
        {
            Name = logGroupIdentifier,
            RetentionInDays = retentionInDays,
            Tags = _stackSetup.Tags,
        });
    }
}