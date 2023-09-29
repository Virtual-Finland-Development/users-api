using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Run database migrations
/// </summary>
public class InitializeDatabaseUser : IAdminAppAction
{
    public async Task Execute(UsersDbContext dataContext, string? credentialsPayload)
    {
        if (credentialsPayload == null)
        {
            throw new ArgumentNullException(nameof(credentialsPayload));
        }
        var credentials = JsonSerializer.Deserialize<DatabaseUserCredentials>(credentialsPayload, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Invalid credentials payload", nameof(credentialsPayload));

        await dataContext.Database.ExecuteSqlRawAsync(
            $"CREATE USER '{credentials.Username}' WITH PASSWORD '{credentials.Password}';");
        await dataContext.Database.ExecuteSqlRawAsync(
            $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA 'public' TO '{credentials.Username}';");
    }

    public record DatabaseUserCredentials(string Username, string Password);
}