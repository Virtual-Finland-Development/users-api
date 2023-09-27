
using Pulumi;
using Pulumi.Aws.CloudWatch;
using Pulumi.Aws.Lambda;

using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class CloudWatch
{
    private readonly StackSetup _stackSetup;
    public CloudWatch(StackSetup stackSetup) => _stackSetup = stackSetup;

    public Output<string> EnsureLambdaFunctionLogGroup(Function lambdaFunction)
    {
        // Configure log retention for new installations
        var lgName = Output.Format($"/aws/lambda/{lambdaFunction.Name}");
        return EnsureLogGroup(lgName);
    }

    public Output<string> EnsureLogGroup(Output<string> logGroupName)
    {
        // Configure log retention for new installations
        logGroupName.Apply(lgName =>
        {
            var existingLogGroup = GetLogGroup.Invoke(new GetLogGroupInvokeArgs
            {
                Name = logGroupName,
            });

            existingLogGroup.Apply(existing =>
            {
                // Create log group only if it doesn't exist
                if (existing == null)
                {
                    var lg = new LogGroup(lgName, new LogGroupArgs
                    {
                        Name = lgName,
                        RetentionInDays = 30,
                        Tags = _stackSetup.Tags
                    });

                    return lg;
                }
                return null;
            });

            return lgName;
        });

        return logGroupName;
    }
}