using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;

namespace VirtualFinland.UserAPI.Helpers;

public interface ICryptoUtility
{
    string Encrypt(string value, string key);
    string Decrypt(string value, string key);
    string Hash(string value);
    void StartQuery(string entityName, string? secretKey);
    string? GetQueryKey(string entityName);
    void ClearQuery(string entityName);

}

public class CryptoUtility : ICryptoUtility
{
    private IDatabaseEncryptionSecrets _secrets;
    private Dictionary<string, string?> _secretQueryKeys = new Dictionary<string, string?>();

    public CryptoUtility(IDatabaseEncryptionSecrets secrets)
    {
        _secrets = secrets;
    }

    public string Encrypt(string value, string secretKey)
    {
        var encryptionProvider = new AesProvider(ResolveEncryptionKey(secretKey), _secrets.EncryptionIV);
        var encryptedBytes = encryptionProvider.Encrypt(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string value, string secretKey)
    {
        var decryptionProvider = new AesProvider(ResolveEncryptionKey(secretKey), _secrets.EncryptionIV);
        var encryptedBytes = Convert.FromBase64String(value);
        var decryptedBytes = decryptionProvider.Decrypt(encryptedBytes);
        return Encoding.UTF8.GetString(decryptedBytes).Trim('\0');
    }

    // Very lazy hash for test, @TODO
    public string Hash(string value)
    {
        return Encrypt(value, Encoding.UTF8.GetString(_secrets.EncryptionIV));
    }

    private byte[] ResolveEncryptionKey(string secretKey)
    {
        using (HashAlgorithm hash = MD5.Create())
            return hash.ComputeHash(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(_secrets.EncryptionKey) + "::" + secretKey));
    }

    public void StartQuery(string entityName, string? secretKey)
    {
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));
        _secretQueryKeys[entityName] = secretKey;
    }

    public string? GetQueryKey(string entityName)
    {
        return _secretQueryKeys.ContainsKey(entityName) ? _secretQueryKeys[entityName] : null;
    }

    public void ClearQuery(string entityName)
    {
        _secretQueryKeys[entityName] = null;
    }
}