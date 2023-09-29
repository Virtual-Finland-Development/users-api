using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

public interface IAdminAppAction
{
    Task Execute(UsersDbContext dataContext, string? data);
}