using Pulumi;
using static VirtualFinland.UsersAPI.Deployment.UsersApiStack;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates the users-api database credentials in AWS Secrets Manager
/// </summary>
public class SecretsManager
{
    public SecretsManager(Config config, StackSetup stackSetup, PostgresDatabase dbConfigs)
    {
        var secretsManagerSecret = new Pulumi.Aws.SecretsManager.Secret($"{stackSetup.ProjectName}-dbConnectionStringSecret-{stackSetup.Environment}");
        new Pulumi.Aws.SecretsManager.SecretVersion($"{stackSetup.ProjectName}-dbConnectionStringSecretVersion-{stackSetup.Environment}", new()
        {
            SecretId = secretsManagerSecret.Id,
            SecretString = Output.All(dbConfigs.DbHostName, dbConfigs.DbPassword)
                .Apply(pulumiOutputs => $"Host={pulumiOutputs[0]};Database={config.Require("dbName")};Username={config.Require("dbAdmin")};Password={pulumiOutputs[1]}"),
        });
        Name = secretsManagerSecret.Name;
        Arn = secretsManagerSecret.Arn;
    }
    public Output<string> Name = default!;
    public Output<string> Arn = default!;
}