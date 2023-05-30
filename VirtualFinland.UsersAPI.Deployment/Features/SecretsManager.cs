using Pulumi;
using Pulumi.Aws.SecretsManager;
using VirtualFinland.UsersAPI.Deployment.Common.Models;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database credentials in AWS Secrets Manager
/// </summary>
public class SecretsManager
{
    public SecretsManager(Config config, StackSetup stackSetup, PostgresDatabase dbConfigs)
    {
        var secret = new Secret($"{stackSetup.ProjectName}-dbConnectionStringSecret-{stackSetup.Environment}");
        new SecretVersion($"{stackSetup.ProjectName}-dbConnectionStringSecretVersion-{stackSetup.Environment}", new()
        {
            SecretId = secret.Id,
            SecretString = dbConfigs.DbConnectionString,
        });
        Name = secret.Name;
        Arn = secret.Arn;
    }
    public Output<string> Name = default!;
    public Output<string> Arn = default!;
}