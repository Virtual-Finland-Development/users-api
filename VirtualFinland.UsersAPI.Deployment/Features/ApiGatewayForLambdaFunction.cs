using Pulumi;
using Pulumi.Aws.ApiGatewayV2;
using Pulumi.Aws.ApiGatewayV2.Inputs;
using Pulumi.Aws.Lambda;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

class ApiGatewayForLambdaFunction
{
    public ApiGatewayForLambdaFunction(StackSetup stackSetup, UsersApiLambdaFunction lambdaFunction)
    {
        // Create aws api gateway v2 for the lambda function
        var apiGatewayV2 = new Api($"{stackSetup.ProjectName}-ApiGatewayV2-{stackSetup.Environment}", new()
        {
            ProtocolType = "HTTP",
            Tags = stackSetup.Tags
        });

        // Create permission for the lambda function to be invoked by the api gateway
        new Permission($"{stackSetup.ProjectName}-LambdaPermission-{stackSetup.Environment}", new()
        {
            Action = "lambda:InvokeFunction",
            Function = lambdaFunction.LambdaFunctionArn,
            Principal = "apigateway.amazonaws.com",
            SourceArn = Output.Format($"{apiGatewayV2.ExecutionArn}/*/*/*")
        }, new CustomResourceOptions
        {
            DependsOn = new InputList<Resource>
            {
                apiGatewayV2,
                lambdaFunction.LambdaFunctionResource
            }
        });

        // Create the integration for the lambda function
        var lambdaIntegration = new Integration($"{stackSetup.ProjectName}-LambdaIntegration-{stackSetup.Environment}", new()
        {
            ApiId = apiGatewayV2.Id,
            IntegrationType = "AWS_PROXY",
            IntegrationUri = lambdaFunction.LambdaFunctionArn,
            PayloadFormatVersion = "2.0",
            PassthroughBehavior = "WHEN_NO_MATCH"
        });

        // Create the route for the api gateway
        var apiGatewayRoute = new Route($"{stackSetup.ProjectName}-ApiGatewayRoute-{stackSetup.Environment}", new()
        {
            ApiId = apiGatewayV2.Id,
            RouteKey = "ANY /{proxy+}",
            Target = Output.Format($"integrations/{lambdaIntegration.Id}")
        });

        // Create stage for the api gateway
        var apiGatewayStage = new Stage($"{stackSetup.ProjectName}-ApiGatewayStage-{stackSetup.Environment}", new()
        {
            ApiId = apiGatewayV2.Id,
            AutoDeploy = true,
            Name = "$default",
            RouteSettings = new()
            {
                new StageRouteSettingArgs() {
                    RouteKey = "ANY /{proxy+}",
                    ThrottlingRateLimit = 10000,
                    ThrottlingBurstLimit = 5000
                },
            }
        });

        ApiGatewayV2Resource = apiGatewayV2;
        ApplicationUrl = apiGatewayV2.ApiEndpoint;
    }

    public Api ApiGatewayV2Resource = default!;
    public Output<string> ApplicationUrl = default!;
}