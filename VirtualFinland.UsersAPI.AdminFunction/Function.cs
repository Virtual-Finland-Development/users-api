using Microsoft.Extensions.DependencyInjection;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.AdminFunction.AdminApp;
using VirtualFinland.AdminFunction.AdminApp.Models;
using System.Text.Json;
using VirtualFinland.AdminFunction.AdminApp.Actions;

namespace VirtualFinland.AdminFunction;

public class Function
{
    public static async Task FunctionHandler(string lambdaEvent)
    {
        // Parse payload
        var payload = JsonSerializer.Deserialize<FunctionPayload>(lambdaEvent) ?? throw new ArgumentNullException(nameof(lambdaEvent));

        // Setup
        using var app = await App.Build();
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Administrate the command
        var action = App.ResolveAction(payload.Action);
        await action.Execute(dataContext, payload.Data);
    }
}