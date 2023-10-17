using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.AdminFunction.AdminApp.Actions;

namespace VirtualFinland.AdminFunction.AdminApp;

public class App
{
    public static async Task<IHost> Build()
    {
        var builder = Host.CreateDefaultBuilder();
        AwsConfigurationManager awsConfigurationManager = new();

        var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
            ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
            : null;
        var dbConnectionString = databaseSecret ?? new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build().GetConnectionString("DefaultConnection");

        builder.ConfigureServices(
            services =>
            {
                services.AddDbContext<UsersDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        op => op
                            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>())
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    );
                });
            });

        return builder.Build();
    }

    public static IAdminAppAction ResolveAction(Models.Actions action)
    {
        return action switch
        {
            Models.Actions.Migrate => new DatabaseMigrationAction(),
            Models.Actions.InitializeDatabaseAuditLogTriggers => new DatabaseAuditLogTriggersInitializationAction(),
            Models.Actions.InitializeDatabaseUser => new DatabaseUserInitializationAction(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }
}