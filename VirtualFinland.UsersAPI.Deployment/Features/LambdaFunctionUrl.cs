using Pulumi;
using Pulumi.Aws.Lambda;
using Pulumi.Command.Local;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class LambdaFunctionUrl
{
    public LambdaFunctionUrl(StackSetup stackSetup, UsersApiLambdaFunction lambdaFunction)
    {

        var functionUrl = new FunctionUrl($"{stackSetup.ProjectName}-FunctionUrl-{stackSetup.Environment}", new FunctionUrlArgs
        {
            FunctionName = lambdaFunction.LambdaFunctionArn,
            AuthorizationType = "NONE"
        });

        new Command($"{stackSetup.ProjectName}-AddPermissions-{stackSetup.Environment}", new CommandArgs
        {
            Create = Output.Format(
                $"aws lambda add-permission --function-name {lambdaFunction.LambdaFunctionArn} --action lambda:InvokeFunctionUrl --principal '*' --function-url-auth-type NONE --statement-id FunctionUrlGrantAccess")
        }, new CustomResourceOptions
        {
            DeleteBeforeReplace = true,
            DependsOn = new InputList<Resource>
            {
                lambdaFunction.LambdaFunctionResource
            }
        });

        ApplicationUrl = functionUrl.FunctionUrlResult;
    }

    public Output<string> ApplicationUrl = default!;
}