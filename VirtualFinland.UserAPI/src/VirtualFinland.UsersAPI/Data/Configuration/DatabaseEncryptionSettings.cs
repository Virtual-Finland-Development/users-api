public interface IDatabaseEncryptionSettings
{
    string EncryptionKey { get; }
    string EncryptionIV { get; }
    bool IsEnabled { get; }
}

public class DatabaseEncryptionSettings : IDatabaseEncryptionSettings
{
    public string EncryptionKey { get; }
    public string EncryptionIV { get; }
    public bool IsEnabled { get; } = true;

    public DatabaseEncryptionSettings(string? encryptionKey, string? encryptionIV, bool isEnabled)
    {
        IsEnabled = isEnabled;
        if (!IsEnabled)
            return;

        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentNullException(nameof(encryptionKey), $"{nameof(encryptionKey)} is null or empty");
        if (string.IsNullOrEmpty(encryptionIV))
            throw new ArgumentNullException(nameof(encryptionIV), $"{nameof(encryptionIV)} is null or empty");
        EncryptionKey = encryptionKey;
        EncryptionIV = encryptionIV;
    }

    public DatabaseEncryptionSettings(string? encryptionKey, string? encryptionIV)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentNullException(nameof(encryptionKey), $"{nameof(encryptionKey)} is null or empty");
        if (string.IsNullOrEmpty(encryptionIV))
            throw new ArgumentNullException(nameof(encryptionIV), $"{nameof(encryptionIV)} is null or empty");
        EncryptionKey = encryptionKey;
        EncryptionIV = encryptionIV;
    }
}