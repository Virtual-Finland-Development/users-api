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
using VirtualFinland.UserAPI.Helpers.Services;
using Amazon.SQS;
using VirtualFinland.UserAPI.Helpers;

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
                services.AddTransient<IAmazonSQS, AmazonSQSClient>();
                services.AddSingleton<NotificationsConfig>();
                services.AddSingleton<EmailTemplates>();
                services.AddSingleton<NotificationService>();
                services.AddSingleton<CleanupConfig>();

                services.AddSingleton<ActivityTriggerService>();
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
                services.AddTransient<InitializeDatabaseAction>();
                services.AddTransient<MigrateAction>();
                services.AddTransient<InitializeDatabaseAuditLogTriggersAction>();
                services.AddTransient<InitializeDatabaseUserAction>();
                services.AddTransient<UpdateTermsOfServiceAction>();
                services.AddTransient<UpdateAnalyticsAction>();
                services.AddTransient<InvalidateCachesAction>();
                services.AddTransient<RunCleanupsAction>();
                services.AddTransient<UpdatePersonActivityAction>();
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
            Models.Actions.InitializeDatabase => scope.ServiceProvider.GetRequiredService<InitializeDatabaseAction>(),
            Models.Actions.Migrate => scope.ServiceProvider.GetRequiredService<MigrateAction>(),
            Models.Actions.InitializeDatabaseAuditLogTriggers => scope.ServiceProvider.GetRequiredService<InitializeDatabaseAuditLogTriggersAction>(),
            Models.Actions.InitializeDatabaseUser => scope.ServiceProvider.GetRequiredService<InitializeDatabaseUserAction>(),
            Models.Actions.UpdateTermsOfService => scope.ServiceProvider.GetRequiredService<UpdateTermsOfServiceAction>(),
            Models.Actions.UpdateAnalytics => scope.ServiceProvider.GetRequiredService<UpdateAnalyticsAction>(),
            Models.Actions.InvalidateCaches => scope.ServiceProvider.GetRequiredService<InvalidateCachesAction>(),
            Models.Actions.RunCleanups => scope.ServiceProvider.GetRequiredService<RunCleanupsAction>(),
            Models.Actions.UpdatePersonActivity => scope.ServiceProvider.GetRequiredService<UpdatePersonActivityAction>(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }
}