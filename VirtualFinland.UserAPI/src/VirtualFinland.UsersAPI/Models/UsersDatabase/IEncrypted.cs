namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public interface IEncrypted : ICloneable
{
    public string? EncryptionKey { get; set; }
}
