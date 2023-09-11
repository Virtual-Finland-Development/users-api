using System.Reflection;
using System.Text.Json;
using VirtualFinland.AdminFunction;
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
            case "update-terms-of-service":
                await UpdateTermsOfServices();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    private static async Task Migrate()
    {
        await Function.FunctionHandler(JsonSerializer.Serialize(new FunctionPayload
        {
            Action = Actions.Migrate,
        }));
    }

    private static async Task UpdateTermsOfServices()
    {
        // Read terms of service from json file
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException();
        string archiveFolder = Path.Combine(currentDirectory, "terms-of-services.json");

        var termsOfServiceData = File.ReadAllText(archiveFolder);

        await Function.FunctionHandler(JsonSerializer.Serialize(new FunctionPayload
        {
            Action = Actions.UpdateTermsOfService,
            Data = termsOfServiceData
        }));
    }
}