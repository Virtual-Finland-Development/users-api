using Pulumi;
using Pulumi.Aws.SecretsManager;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Pulumi.Aws.Iam;

namespace VirtualFinland.UsersAPI.Deployment.Features;

public class SecretsManager
{
    public SecretsManager(Config config, StackSetup stackSetup, string secretName, Output<string> secretValue)
    {
        var secret = new Secret($"{stackSetup.ProjectName}-{secretName}-{stackSetup.Environment}");
        new SecretVersion($"{stackSetup.ProjectName}-{secretName}Version-{stackSetup.Environment}", new()
        {
            SecretId = secret.Id,
            SecretString = secretValue,
        });

        ReadPolicy = new Policy($"{stackSetup.ProjectName}-{secretName}-SecretManagerPolicy-{stackSetup.Environment}", new()
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
                            ""{secret.Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = stackSetup.Tags,
        });

        Name = secret.Name;
        Arn = secret.Arn;
    }

    public Output<string> Name = default!;
    public Output<string> Arn = default!;
    public Policy ReadPolicy = default!;
}