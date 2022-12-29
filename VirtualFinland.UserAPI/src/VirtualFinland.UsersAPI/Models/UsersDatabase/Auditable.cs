namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public abstract class Auditable
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
