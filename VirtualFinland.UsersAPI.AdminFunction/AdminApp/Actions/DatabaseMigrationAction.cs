using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Run database migrations
/// </summary>
public class DatabaseMigrationAction : IAdminAppAction
{
    public static async Task Execute(UsersDbContext dataContext, string? _)
    {
        await dataContext.Database.MigrateAsync();
    }
}