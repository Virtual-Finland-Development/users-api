using Microsoft.Extensions.DependencyInjection;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.AdminFunction.AdminApp;
using VirtualFinland.AdminFunction.AdminApp.Models;
using Amazon.Lambda.Core;
using Amazon.Lambda.CloudWatchEvents;
using Newtonsoft.Json.Linq;
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace VirtualFinland.AdminFunction;

public class Function
{
    public static async Task FunctionHandler(FunctionPayload payload)
    {
        // Setup
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Administrate the command
        var action = App.ResolveAction(payload.Action);
        await action.Execute(dataContext, payload.Data);
    }

    public static async Task FunctionHandler(CloudWatchEvent<JObject> cloudWatchLogsEvent)
    {
        // Setup
        var payload = cloudWatchLogsEvent.Detail.ToObject<FunctionPayload>() ?? throw new ArgumentException("Received invalid payload");
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Administrate the command
        var action = App.ResolveAction(payload.Action);

        // Restrict cloudwatch event to only run the specific actions
        var allowedActions = new List<Actions> { Actions.UpdateAnalytics };
        if (!allowedActions.Contains(payload.Action))
        {
            throw new ArgumentException("Received invalid action");
        }

        await action.Execute(dataContext, payload.Data);
    }
}