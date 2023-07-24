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

    public DatabaseEncryptionSecrets(string encryptionKey, string encryptionIV)
    {
        EncryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
        EncryptionIV = Encoding.UTF8.GetBytes(encryptionIV);
    }
}