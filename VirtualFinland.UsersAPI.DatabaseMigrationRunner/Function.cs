using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;

namespace DatabaseMigrationRunner;

public class Function
{
    public async Task FunctionHandler()
    {
        var builder = Host.CreateDefaultBuilder();

        AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();

        var dbConnectionString = await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"));
        builder.ConfigureServices(
            services =>
            {
                services.AddDbContext<UsersDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        op => op.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), new List<string>()));
                });
            });

        using (var app = builder.Build())
        {
            using (var scope = app.Services.CreateScope())
            {
                Log.Information("Migrate database");

                var dataContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
                await dataContext.Database.MigrateAsync();

                Log.Information("Database migration completed");
            }
        }
    }
}