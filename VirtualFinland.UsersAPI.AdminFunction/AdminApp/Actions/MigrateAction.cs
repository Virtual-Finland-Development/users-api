using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Run database migrations
/// </summary>
public class MigrateAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    public MigrateAction(UsersDbContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task Execute(string? _)
    {
        await _dataContext.Database.MigrateAsync();
    }
}