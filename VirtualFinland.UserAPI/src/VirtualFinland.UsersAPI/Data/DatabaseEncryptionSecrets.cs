public interface IDatabaseEncryptionSecrets
{
    string EncryptionKey { get; }
    string EncryptionIV { get; }
}

public class DatabaseEncryptionSecrets : IDatabaseEncryptionSecrets
{
    public string EncryptionKey { get; }
    public string EncryptionIV { get; }


    public DatabaseEncryptionSecrets(string? encryptionKey, string? encryptionIV)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentNullException(nameof(encryptionKey), $"{nameof(encryptionKey)} is null or empty");
        if (string.IsNullOrEmpty(encryptionIV))
            throw new ArgumentNullException(nameof(encryptionIV), $"{nameof(encryptionIV)} is null or empty");
        EncryptionKey = encryptionKey;
        EncryptionIV = encryptionIV;
    }
}