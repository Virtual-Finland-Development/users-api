using System.Text.Json;
using VirtualFinland.AdminFunction;
using VirtualFinland.AdminFunction.AdminApp.Actions;
using VirtualFinland.AdminFunction.AdminApp.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var command = args[0] ?? throw new ArgumentNullException(nameof(args));
        var action = (Actions)Enum.Parse(typeof(Actions), command.Replace("-", ""), true);
        var payload = ResolveFunctionPayload(action);
        await Function.FunctionHandler(payload);
    }

    private static FunctionPayload ResolveFunctionPayload(Actions action)
    {
        if (action == Actions.InitializeDatabaseUser || action == Actions.InitializeDatabase)
        {
            return new FunctionPayload
            {
                Action = action,
                Data = JsonSerializer.Serialize(new InitializeDatabaseUserAction.DatabaseUserCredentials(
                    Environment.GetEnvironmentVariable("DATABASE_USER") ?? throw new ArgumentException(nameof(Environment.GetEnvironmentVariable)),
                    Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? throw new ArgumentException(nameof(Environment.GetEnvironmentVariable))
                ))
            };
        }

        return new FunctionPayload
        {
            Action = action,
        };
    }
}