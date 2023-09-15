using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTriggers : Migration
    {
        private string[] _loggingTables = new[] {
            "Persons",
            "PersonAdditionalInformation",
            "WorkPreferences",
            "Skills",
            "Permits",
            "Languages",
            "Educations",
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION audit_trigger_func()
                RETURNS trigger AS $body$
                DECLARE
                    changed_columns text[] := '{}';
                    tmp_column_name text;
                    new_value text;
                    old_value text;
                BEGIN
                    if (TG_OP = 'INSERT') then
                        RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%""}}', 
                            TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr();
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
                            RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%""}, columns: ""%""}', 
                                TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr(), changed_columns;
                        END IF;
                        RETURN NEW;
                    elsif (TG_OP = 'DELETE') then
                        RAISE LOG 'AuditLog: {action: ""%"", table: ""%"", id: ""%"", timestamp: ""%"", session: {user: ""%"", ip: ""%""}}', 
                            TG_OP, TG_TABLE_NAME::text, NEW.""Id"", current_timestamp, session_user::text, inet_client_addr();
                        RETURN OLD;
                    end if;
                END;
                $body$
                LANGUAGE plpgsql
            ");

            foreach (var table in _loggingTables)
            {
                migrationBuilder.Sql(@$"
                    CREATE TRIGGER ""{table}_audit_trigger""
                        AFTER INSERT OR UPDATE OR DELETE ON ""{table}""
                        FOR EACH ROW EXECUTE FUNCTION audit_trigger_func();
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in _loggingTables)
            {
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS \"{table}_audit_trigger\" ON \"{table}\";");
            }
        }
    }
}
