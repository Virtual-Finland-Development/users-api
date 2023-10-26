using Microsoft.Extensions.DependencyInjection;
using VirtualFinland.AdminFunction.AdminApp;
using VirtualFinland.AdminFunction.AdminApp.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using Amazon.Lambda.CloudWatchEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace VirtualFinland.AdminFunction;

public class Function
{
    public static async Task FunctionHandler(FunctionPayload payload)
    {
        // Setup
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();

        // Administrate the command
        var action = scope.ResolveAction(payload.Action);
        await action.Execute(payload.Data);
    }

    public static async Task FunctionHandler(SQSEvent sqsEvent)
    {
        // Setup
        var payloadBody = sqsEvent.Records.First().Body;
        var payload = JsonSerializer.Deserialize<FunctionPayload>(payloadBody) ?? throw new Exception("Could not deserialize payload");
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();

        ValidateEventSourceAction(payload.Action);

        // Administrate the command
        var action = scope.ResolveAction(payload.Action);
        await action.Execute(payload.Data);
    }

    public static async Task FunctionHandler(CloudWatchEvent<object> cloudWatchEvent)
    {
        // Setup
        var payloadBody = cloudWatchEvent.Detail.ToString() ?? throw new Exception("Could not get payload");
        var payload = JsonSerializer.Deserialize<FunctionPayload>(payloadBody) ?? throw new Exception("Could not deserialize payload");
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();

        ValidateEventSourceAction(payload.Action);

        // Administrate the command
        var action = scope.ResolveAction(payload.Action);
        await action.Execute(payload.Data);
    }

    private static void ValidateEventSourceAction(Actions action)
    {
        // Ensure only specific actions are allowed to be invoked from CloudWatch / Sqs events
        var allowedActions = new[] { Actions.UpdateAnalytics };
        if (!allowedActions.Contains(action))
        {
            throw new Exception($"Action '{action}' is not allowed to be invoked from event source");
        }
    }
}