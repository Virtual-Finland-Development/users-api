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
        await Function.FunctionHandler(new FunctionPayload
        {
            Action = Actions.Migrate,
        });
    }

    private static async Task UpdateTermsOfServices()
    {
        await Function.FunctionHandler(new FunctionPayload
        {
            Action = Actions.UpdateTermsOfService,
        });
    }
}