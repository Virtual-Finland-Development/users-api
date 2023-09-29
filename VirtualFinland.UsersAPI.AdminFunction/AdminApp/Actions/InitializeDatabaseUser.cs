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

        await dataContext.Database.ExecuteSqlRawAsync(@$"
            -- Create role if not exists
            DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'user') THEN CREATE ROLE user; END IF; END $$;
            -- Grant permissions to the role
            DO $$ BEGIN IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'user') THEN GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA 'public' TO user; END IF; END $$;
            -- Remove all users from user role
            DO $$ BEGIN FOR r IN (SELECT * FROM pg_user) LOOP IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = r.usename) THEN REVOKE user FROM r.usename; END IF; END LOOP; END $$;
            -- Create user
            CREATE USER '{credentials.Username}' WITH PASSWORD '{credentials.Password}';
            -- Add user to user role
            GRANT user TO '{credentials.Username}';
        ");
    }

    public record DatabaseUserCredentials(string Username, string Password);
}