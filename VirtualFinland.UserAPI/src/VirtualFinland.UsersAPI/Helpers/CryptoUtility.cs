
namespace VirtualFinland.UserAPI.Helpers;

public interface ICryptoUtility
{
    string Encrypt(string value, string key);
    string Decrypt(string value, string key);
    string Hash(string value);
    void PrepareQuery(string secretKey);
    string ResolveQuery();

}

public class CryptoUtility : ICryptoUtility
{
    private IDatabaseEncryptionSecrets _secrets;
    private string? _secretQueryKey = null;

    public CryptoUtility(IDatabaseEncryptionSecrets secrets)
    {
        _secrets = secrets;
    }

    public string Encrypt(string value, string key)
    {
        return value;
    }

    public string Decrypt(string value, string key)
    {
        return value;
    }

    public string Hash(string value)
    {
        return value;
    }

    public void PrepareQuery(string secretKey)
    {
        if (_secretQueryKey != null)
        {
            throw new ArgumentException("Previous query key not resolved");
        }
        _secretQueryKey = secretKey;
    }

    public string ResolveQuery()
    {
        if (_secretQueryKey == null)
        {
            throw new ArgumentException("Secret key not set for the query");
        }
        var queryKey = _secretQueryKey;
        _secretQueryKey = null;
        return queryKey;
    }
}