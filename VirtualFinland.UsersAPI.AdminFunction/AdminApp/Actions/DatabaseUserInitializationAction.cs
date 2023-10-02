using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Run database migrations
/// </summary>
public class DatabaseUserInitializationAction : IAdminAppAction
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
            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'appusers')
                THEN 
                    CREATE ROLE appusers; 
                END IF; 
            END $$;
            -- Grant permissions to the role
            DO $$ 
            BEGIN 
                GRANT SELECT, INSERT, UPDATE, DELETE, TRIGGER ON ALL TABLES IN SCHEMA public TO appusers;
            END $$;
            -- Remove all users from user role
            DO $$ 
            DECLARE
                username text;
            BEGIN 
                FOR username IN (
                    SELECT pg_user.usename
                    FROM pg_user
                    WHERE usesysid IN (
                        SELECT member
                        FROM pg_auth_members
                        WHERE roleid = (
                            SELECT oid
                            FROM pg_roles
                            WHERE rolname = 'appusers'
                        )
                    )
                ) 
                LOOP 
                    EXECUTE 'DROP USER ' || username;
                END LOOP; 
            END $$;
            -- Create user
            CREATE USER {credentials.Username} WITH PASSWORD '{credentials.Password}';
            -- Add user to user role
            GRANT appusers TO {credentials.Username};
        ");
    }

    public record DatabaseUserCredentials(string Username, string Password);
}