using Microsoft.EntityFrameworkCore;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Extensions;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Sets up the database user for the application. 
/// The intented use is to allow the pulumi deployment process to create and manage the application level database user.
/// In live environments the script is called during a deployment (finishing phases) from a cloud function residing in the same virtual private cloud (VPC) as the database.
/// </summary>
public class DatabaseAuditLogTriggersInitializationAction : IAdminAppAction
{
    public async Task Execute(UsersDbContext dataContext, string? _)
    {
        var loggingTables = dataContext.GetDbSetEntityTypes()
            .Where(e => e.ClrType.GetInterfaces().Contains(typeof(Auditable)))
            .Select(e => e.GetTableName())
            .ToList();

        Console.WriteLine("Creating audit trigger function..");

        await dataContext.Database.ExecuteSqlRawAsync(@"
                CREATE OR REPLACE FUNCTION audit_trigger_func()
                RETURNS trigger AS $body$
                DECLARE
                    changed_columns text[] := '{}';
                    tmp_column_name text;
                    new_value text;
                    old_value text;
                BEGIN
                    if (TG_OP = 'INSERT') then
                        RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%"", meta: ""%""}}', 
                            TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr(), NEW.""Metadata"";
                        RETURN NEW;
                    elsif (TG_OP = 'UPDATE') then
                        -- Check each column in the table
                        FOR tmp_column_name IN SELECT column_name FROM information_schema.columns WHERE table_name = TG_TABLE_NAME LOOP
                            -- Construct dynamic SQL to compare the NEW and OLD values for each column
                            EXECUTE format('SELECT ($1).%I, ($2).%I', tmp_column_name, tmp_column_name)
                            INTO new_value, old_value
                            USING NEW, OLD;

                            -- Compare the values and add the column name to the array if they are distinct
                            IF new_value IS DISTINCT FROM old_value THEN
                                changed_columns := array_append(changed_columns, tmp_column_name);
                            END IF;
                        END LOOP;

                        -- Return the array of changed column names
                        IF array_length(changed_columns, 1) > 0 THEN
                            RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%"", meta: ""%""}, columns: ""%""}', 
                                TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr(), NEW.""Metadata"", changed_columns;
                        END IF;
                        RETURN NEW;
                    elsif (TG_OP = 'DELETE') then
                        RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%"", meta: ""%""}}', 
                            TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr(), NEW.""Metadata"";
                        RETURN OLD;
                    end if;
                END;
                $body$
                LANGUAGE plpgsql
            ".Replace("{", "{{").Replace("}", "}}")); // Curly escapes by: https://github.com/dotnet/efcore/issues/30188#issuecomment-1411763443

        foreach (var table in loggingTables)
        {
            Console.WriteLine($"Creating audit trigger for table {table}");
            await dataContext.Database.ExecuteSqlRawAsync(@$"
                DROP TRIGGER IF EXISTS ""{table}_audit_trigger"" ON ""{table}"";
                CREATE TRIGGER ""{table}_audit_trigger""
                    AFTER INSERT OR UPDATE OR DELETE ON ""{table}""
                    FOR EACH ROW EXECUTE FUNCTION audit_trigger_func();
            ");
        }

    }
}