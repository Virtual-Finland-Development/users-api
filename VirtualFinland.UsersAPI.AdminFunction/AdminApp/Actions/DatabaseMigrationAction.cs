using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Run database migrations
/// </summary>
public class DatabaseMigrationAction : IAdminAppAction
{
    private readonly UsersDbContext _dataContext;
    public DatabaseMigrationAction(UsersDbContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task Execute(string? _)
    {
        await _dataContext.Database.MigrateAsync();
    }
}