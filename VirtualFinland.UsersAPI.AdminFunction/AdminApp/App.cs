using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.AdminFunction.AdminApp.Actions;
using Amazon.CloudWatch;
using StackExchange.Redis;
using VirtualFinland.UserAPI.Data.Repositories;

namespace VirtualFinland.AdminFunction.AdminApp;

public class App
{
    public static async Task<IHost> Build()
    {
        var builder = Host.CreateDefaultBuilder();
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        // Database
        AwsConfigurationManager awsConfigurationManager = new();
        var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
            ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
            : null;
        var dbConnectionString = databaseSecret ?? configurationBuilder.GetConnectionString("DefaultConnection");

        // Cache provider
        var redisEndpoint = Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? configurationBuilder["Redis:Endpoint"];
        ConnectionMultiplexer redisCluster = ConnectionMultiplexer.Connect($"{redisEndpoint},abortConnect=false,connectRetry=5");
        IDatabase redisDatabase = redisCluster.GetDatabase();

        builder.ConfigureServices(
            services =>
            {
                // Dependencies
                services.AddDbContext<UsersDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        op => op
                            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>())
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    );
                });
                services.AddTransient<IAmazonCloudWatch, AmazonCloudWatchClient>();
                services.AddSingleton<AnalyticsConfig>();
                services.AddSingleton(redisDatabase);
                services.AddSingleton<ICacheRepositoryFactory, CacheRepositoryFactory>();

                // Actions
                services.AddTransient<DatabaseMigrationAction>();
                services.AddTransient<DatabaseAuditLogTriggersInitializationAction>();
                services.AddTransient<DatabaseUserInitializationAction>();
                services.AddTransient<TermsOfServiceUpdateAction>();
                services.AddTransient<UpdateAnalyticsAction>();
                services.AddTransient<InvalidateCachesAction>();
            });

        return builder.Build();
    }
}

public static class AppExtensions
{
    public static IAdminAppAction ResolveAction(this IServiceScope scope, Models.Actions action)
    {
        return action switch
        {
            Models.Actions.Migrate => scope.ServiceProvider.GetRequiredService<DatabaseMigrationAction>(),
            Models.Actions.InitializeDatabaseAuditLogTriggers => scope.ServiceProvider.GetRequiredService<DatabaseAuditLogTriggersInitializationAction>(),
            Models.Actions.InitializeDatabaseUser => scope.ServiceProvider.GetRequiredService<DatabaseUserInitializationAction>(),
            Models.Actions.UpdateTermsOfService => scope.ServiceProvider.GetRequiredService<TermsOfServiceUpdateAction>(),
            Models.Actions.UpdateAnalytics => scope.ServiceProvider.GetRequiredService<UpdateAnalyticsAction>(),
            Models.Actions.InvalidateCaches => scope.ServiceProvider.GetRequiredService<InvalidateCachesAction>(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }
}