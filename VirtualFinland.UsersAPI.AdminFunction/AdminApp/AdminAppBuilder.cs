using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;

namespace VirtualFinland.AdminFunction.AdminApp;

public class AdminAppBuilder
{
    public static async Task<IHost> Build()
    {
        var builder = Host.CreateDefaultBuilder();

        AwsConfigurationManager awsConfigurationManager = new();

        var dbConnectionString = await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"));

        builder.ConfigureServices(
            services =>
            {
                services.AddSingleton<IAuditInterceptor, AuditInterceptor>();
                services.AddDbContext<UsersDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>()));
                });
            });

        return builder.Build();
    }
}