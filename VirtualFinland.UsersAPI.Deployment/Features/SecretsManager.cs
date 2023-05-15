using Pulumi;
using Pulumi.Aws.Iam;
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
        StackSetup = stackSetup;
        var secret = new Secret($"{stackSetup.ProjectName}-dbConnectionStringSecret-{stackSetup.Environment}");
        new SecretVersion($"{stackSetup.ProjectName}-dbConnectionStringSecretVersion-{stackSetup.Environment}", new()
        {
            SecretId = secret.Id,
            SecretString = Output.All(dbConfigs.DbHostName, dbConfigs.DbPassword)
                .Apply(pulumiOutputs => $"Host={pulumiOutputs[0]};Database={dbConfigs.DbName};Username={dbConfigs.DbUsername};Password={pulumiOutputs[1]}"),
        });

        Name = secret.Name;
        Arn = secret.Arn;
    }

    public Policy GetPolicy()
    {
        return new Policy($"{StackSetup.ProjectName}-LambdaSecretManagerPolicy-{StackSetup.Environment}", new()
        {
            Description = "Users-API Secret Get Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""secretsmanager:GetSecretValue"",
                            ""secretsmanager:DescribeSecret""
                        ],
                        ""Resource"": [
                            ""{Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = StackSetup.Tags,
        });
    }

    public Output<string> Name = default!;
    public Output<string> Arn = default!;
    private readonly StackSetup StackSetup;
}