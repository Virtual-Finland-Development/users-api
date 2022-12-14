namespace VirtualFinland.UserAPI.Models.UsersDatabase;

public interface IEntity
{
    Guid Id { get; set; }
    DateTime Created { get; set; }
    DateTime Modified { get; set; }
}
