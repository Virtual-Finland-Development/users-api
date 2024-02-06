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
using Amazon.SimpleEmail;
using Amazon.Lambda.Core;

namespace VirtualFinland.AdminFunction.AdminApp;

public class App
{
    public static async Task<IHost> BuildAsync()
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

        builder.ConfigureServices(
            services =>
            {
                // Dependencies
                services.AddTransient<IAmazonSQS, AmazonSQSClient>();
                services.AddTransient<IAmazonCloudWatch, AmazonCloudWatchClient>();
                services.AddSingleton<AnalyticsConfig>();
                services.AddSingleton<AnalyticsLoggerFactory>();
                services.AddSingleton<AnalyticsService>();
                services.AddSingleton<NotificationsConfig>();
                services.AddSingleton<EmailTemplates>();
                services.AddSingleton<NotificationService>();
                services.AddSingleton<CleanupConfig>();

                services.AddSingleton<ActionDispatcherService>();
                services.AddDbContext<UsersDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        op => op
                            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>())
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    );
                });
                services.AddSingleton<IPersonRepository, PersonRepository>();
                services.AddSingleton(redisCluster.GetDatabase());
                services.AddSingleton<ICacheRepositoryFactory, CacheRepositoryFactory>();
                services.AddTransient<AmazonSimpleEmailServiceClient>();

                // Actions
                services.AddTransient<InitializeDatabaseAction>();
                services.AddTransient<MigrateAction>();
                services.AddTransient<InitializeDatabaseAuditLogTriggersAction>();
                services.AddTransient<InitializeDatabaseUserAction>();
                services.AddTransient<UpdateTermsOfServiceAction>();
                services.AddTransient<UpdateAnalyticsAction>();
                services.AddTransient<InvalidateCachesAction>();
                services.AddTransient<RunCleanupsAction>();
                services.AddTransient<UpdatePersonAction>();
                services.AddTransient<SendEmailAction>();
            });

        return builder.Build();
    }
}

public static class AppExtensions
{
    public static IAdminAppAction ResolveAction(this IServiceScope scope, Models.Actions action)
    {
        var actionName = action.ToString();
        var actionType = Type.GetType($"VirtualFinland.AdminFunction.AdminApp.Actions.{actionName}Action") ?? throw new ArgumentException($"Action {actionName} not found");
        return scope.ServiceProvider.GetService(actionType) as IAdminAppAction ?? throw new ArgumentException($"Action {actionName} could not be resolved");
    }
}