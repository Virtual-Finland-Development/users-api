using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Sets up the database user for the application. 
/// The intented use is to allow the pulumi deployment process to create and manage the application level database user.
/// In live environments the script is called during a deployment (finishing phases) from a cloud function residing in the same virtual private cloud (VPC) as the database.
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

        // Manages the appusers role, permissions and the user that belong to it with a sync-strategy: 
        // - clearing the previous users and then creating the new one
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
            -- Remove all previous users from user role / drop users before creating a new one
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
            -- Add user to the role
            GRANT appusers TO {credentials.Username};
        ");
    }

    public record DatabaseUserCredentials(string Username, string Password);
}