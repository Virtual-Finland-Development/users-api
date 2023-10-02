using System.Text.Json;
using VirtualFinland.AdminFunction;
using VirtualFinland.AdminFunction.AdminApp.Actions;
using VirtualFinland.AdminFunction.AdminApp.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var command = args[0] ?? throw new ArgumentNullException(nameof(args));
        switch (command)
        {
            case "migrate":
                await Migrate();
                break;
            case "initialize-database-user":
                await InitializeDatabaseUser();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    private static async Task Migrate()
    {
        await Function.FunctionHandler(new FunctionPayload
        {
            Action = Actions.Migrate,
        });
    }

    private static async Task InitializeDatabaseUser()
    {
        await Function.FunctionHandler(new FunctionPayload
        {
            Action = Actions.InitializeDatabaseUser,
            Data = JsonSerializer.Serialize(new DatabaseUserInitializationAction.DatabaseUserCredentials(
                Environment.GetEnvironmentVariable("DATABASE_USER") ?? throw new ArgumentNullException(nameof(Environment.GetEnvironmentVariable)),
                Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? throw new ArgumentNullException(nameof(Environment.GetEnvironmentVariable))
            ))
        });
    }
}