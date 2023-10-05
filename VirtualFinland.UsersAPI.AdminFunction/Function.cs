using Microsoft.Extensions.DependencyInjection;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.AdminFunction.AdminApp;
using VirtualFinland.AdminFunction.AdminApp.Models;
using Amazon.Lambda.Core;
using VirtualFinland.Converters;
[assembly: LambdaSerializer(typeof(CaseInsensitiveLambdaJsonSerializer))]

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
}