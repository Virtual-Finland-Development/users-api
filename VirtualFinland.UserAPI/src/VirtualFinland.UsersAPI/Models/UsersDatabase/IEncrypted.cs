namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public interface IEncrypted
{
    public string? EncryptionKey { get; set; }
}
