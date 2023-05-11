using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class EnablePqAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("5346f52e-9927-436a-b515-f566c226b853"));

            migrationBuilder.Sql("CREATE EXTENSION pgaudit;", true);
            migrationBuilder.Sql("ALTER SYSTEM SET pgaudit.log = 'read, write';", true);

            /*  migrationBuilder.Sql("CREATE ROLE auditor NOLOGIN;", true);
             migrationBuilder.Sql("GRANT SELECT, DELETE on public.\"Persons\" to auditor;", true);
             migrationBuilder.Sql("ALTER SYSTEM SET pgaudit.role TO 'auditor';", true); */

            migrationBuilder.Sql("SELECT pg_reload_conf();");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("c21ad2fb-f54f-40d9-a231-1a71ff304ca0"), new DateTime(2023, 5, 11, 9, 2, 52, 97, DateTimeKind.Utc).AddTicks(3146), "95b7e262-8699-41d4-a15f-7566ae01d648", "e418c198-4014-4001-a482-be0ecf21b216", new DateTime(2023, 5, 11, 9, 2, 52, 97, DateTimeKind.Utc).AddTicks(3146), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 5, 11, 9, 2, 52, 97, DateTimeKind.Utc).AddTicks(3063), new DateTime(2023, 5, 11, 9, 2, 52, 97, DateTimeKind.Utc).AddTicks(3064) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("c21ad2fb-f54f-40d9-a231-1a71ff304ca0"));

            migrationBuilder.Sql("ALTER SYSTEM RESET pgaudit.log;", true);
            /* migrationBuilder.Sql("REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM auditor;", true);
            migrationBuilder.Sql("DROP ROLE auditor;", true); */
            migrationBuilder.Sql("DROP EXTENSION pgaudit;", true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("5346f52e-9927-436a-b515-f566c226b853"), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), "31098ce8-537e-4da3-b18d-3a4e0a34e900", "95b22fff-872b-4929-87e8-4db8509b61f3", new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470) });
        }
    }
}
