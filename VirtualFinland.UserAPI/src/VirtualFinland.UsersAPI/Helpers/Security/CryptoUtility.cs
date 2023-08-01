using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;

namespace VirtualFinland.UserAPI.Helpers.Security;

public interface ICryptoUtility
{
    string Encrypt(string? value, string? key);
    string Decrypt(string? value, string? key);
    string Hash(string value);
    string SecretHash(string value);
    CryptoUtilityState State { get; }
    CryptoIdentityHelpers IdentityHelpers { get; }
}

public class CryptoUtility : ICryptoUtility
{
    private IDatabaseEncryptionSecrets _secrets;
    private readonly Dictionary<string, string?> _secretQueryKeys = new();
    public CryptoUtilityState State { get; }
    public CryptoIdentityHelpers IdentityHelpers { get; }

    public CryptoUtility(IDatabaseEncryptionSecrets secrets)
    {
        _secrets = secrets;
        State = new CryptoUtilityState();
        IdentityHelpers = new CryptoIdentityHelpers(this);
    }

    public string Encrypt(string? value, string? secretKey)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        var resolvedKey = ResolveEncryptionKey(secretKey);
        var encryptionProvider = new AesProvider(Encoding.UTF8.GetBytes(resolvedKey), Encoding.UTF8.GetBytes(_secrets.EncryptionIV));
        var encryptedBytes = encryptionProvider.Encrypt(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string? value, string? secretKey)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        try
        {
            var resolvedKey = ResolveEncryptionKey(secretKey);
            var decryptionProvider = new AesProvider(Encoding.UTF8.GetBytes(resolvedKey), Encoding.UTF8.GetBytes(_secrets.EncryptionIV));
            var encryptedBytes = Convert.FromBase64String(value);
            var decryptedBytes = decryptionProvider.Decrypt(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes).Trim('\0');
        }
        catch (CryptographicException)
        {
            throw new ArgumentException("Decryption failure");
        }
    }

    private string ResolveEncryptionKey(string secretKey)
    {
        return Hash($"{_secrets.EncryptionKey}::{secretKey}");
    }

    public string SecretHash(string input)
    {
        return Encrypt(input, _secrets.EncryptionKey);
    }

    public string Hash(string input)
    {
        using MD5 hashAlgorithm = MD5.Create();
        // Convert the input string to a byte array and compute the hash.
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }
}