using System.Text;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;

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
        var encryptionProvider = new AesProvider(_secrets.EncryptionKey, ResolveEncryptionIV(key));
        var encryptedBytes = encryptionProvider.Encrypt(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string value, string key)
    {
        var decryptionProvider = new AesProvider(_secrets.EncryptionKey, ResolveEncryptionIV(key));
        var decryptedBytes = Encoding.UTF8.GetBytes(value);
        return Encoding.UTF8.GetString(decryptionProvider.Decrypt(decryptedBytes)).Trim('\0');
    }

    // Lazy hash for test, @TODO
    public string Hash(string value)
    {
        return Encrypt(value, Encoding.UTF8.GetString(_secrets.EncryptionIV));
    }

    private byte[] ResolveEncryptionIV(string key)
    {
        /* // Ensure key string is exactly 32 bytes
        var iv = key.Substring(0, 16);
        if (iv.Length != 16)
        {
            iv = iv.PadRight(16, '0');
        }
        return Encoding.UTF8.GetBytes(iv); */
        return _secrets.EncryptionIV;
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