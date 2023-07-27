using System.Text;

public interface IDatabaseEncryptionSecrets
{
    byte[] EncryptionKey { get; }
    byte[] EncryptionIV { get; }
}

public class DatabaseEncryptionSecrets : IDatabaseEncryptionSecrets
{
    public byte[] EncryptionKey { get; }
    public byte[] EncryptionIV { get; }

    public DatabaseEncryptionSecrets(byte[] encryptionKey, byte[] encryptionIV)
    {
        EncryptionKey = encryptionKey;
        EncryptionIV = encryptionIV;
    }

    public DatabaseEncryptionSecrets(string? encryptionKey, string? encryptionIV)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentNullException(nameof(encryptionKey), $"{nameof(encryptionKey)} is null or empty");
        if (string.IsNullOrEmpty(encryptionIV))
            throw new ArgumentNullException(nameof(encryptionIV), $"{nameof(encryptionIV)} is null or empty");
        EncryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
        EncryptionIV = Encoding.UTF8.GetBytes(encryptionIV);
    }
}