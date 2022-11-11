using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class AwsConfigurationManager
{
    private readonly AmazonSecretsManagerClient _amazonSecretsManagerClient;
    private readonly SecretsManagerCache _secretsManagerCache;

    public AwsConfigurationManager()
    {
        _amazonSecretsManagerClient = new AmazonSecretsManagerClient();
        _secretsManagerCache = new SecretsManagerCache(_amazonSecretsManagerClient);
    }

    public async Task<string> GetSecretString(string? secretName)
    {
        return await _secretsManagerCache.GetSecretString(secretName);
    }
}