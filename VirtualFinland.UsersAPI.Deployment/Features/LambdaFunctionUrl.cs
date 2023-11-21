using Pulumi;
using Pulumi.Aws.Lambda;
using Pulumi.Command.Local;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class LambdaFunctionUrl
{
    public LambdaFunctionUrl(StackSetup stackSetup, UsersApiLambdaFunction lambdaFunction)
    {

        var functionUrl = new FunctionUrl(stackSetup.CreateResourceName("FunctionUrl"), new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.LambdaFunctionArn,
            AuthorizationType = "NONE"
        });

        ApplicationUrl = functionUrl.FunctionUrlResult;
    }

    public Output<string> ApplicationUrl = default!;
}