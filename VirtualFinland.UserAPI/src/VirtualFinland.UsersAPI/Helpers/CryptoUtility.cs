
namespace VirtualFinland.UserAPI.Helpers;

public interface ICryptoUtility
{
    string Encrypt(string value);
    string Decrypt(string value);
}

public class CryptoUtility : ICryptoUtility
{
    private IDatabaseEncryptionSecrets _secrets;

    public CryptoUtility(IDatabaseEncryptionSecrets secrets)
    {
        _secrets = secrets;
    }

    public string Encrypt(string value)
    {
        return value;
    }

    public string Decrypt(string value)
    {
        return value;
    }
}