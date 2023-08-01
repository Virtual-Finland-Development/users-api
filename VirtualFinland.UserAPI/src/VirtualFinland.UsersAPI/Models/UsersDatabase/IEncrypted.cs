namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public interface IEncrypted : ICloneable
{
    public string? DataAccessKey { get; set; }
}
