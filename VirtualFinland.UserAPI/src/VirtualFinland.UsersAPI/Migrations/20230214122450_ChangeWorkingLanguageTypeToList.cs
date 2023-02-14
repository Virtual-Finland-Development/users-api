using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class ChangeWorkingLanguageTypeToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("fb064d95-0733-4cf0-a836-0642a04fd46b"));

            migrationBuilder.AlterColumn<string>(
                name: "WorkingLanguageEnum",
                table: "WorkPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("e870bcad-b6f2-4dfa-b7c8-c5c9595b7a26"), new DateTime(2023, 2, 14, 12, 24, 50, 429, DateTimeKind.Utc).AddTicks(7405), "f3316a85-a400-4c39-ab42-66afaa1d67be", "e667371b-0499-4e3b-82e8-d5d6959c68b7", new DateTime(2023, 2, 14, 12, 24, 50, 429, DateTimeKind.Utc).AddTicks(7405), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 2, 14, 12, 24, 50, 429, DateTimeKind.Utc).AddTicks(7323), new DateTime(2023, 2, 14, 12, 24, 50, 429, DateTimeKind.Utc).AddTicks(7323) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("e870bcad-b6f2-4dfa-b7c8-c5c9595b7a26"));

            // Regress the comma separated list data back to single value
            // Take first two characters of comma separated list as enum value
            migrationBuilder.Sql(@"
                UPDATE ""WorkPreferences""
                SET ""WorkingLanguageEnum"" = substring(""WorkingLanguageEnum"", 1, 2) 
                WHERE ""WorkingLanguageEnum"" IS NOT NULL");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingLanguageEnum",
                table: "WorkPreferences",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("fb064d95-0733-4cf0-a836-0642a04fd46b"), new DateTime(2023, 2, 8, 13, 4, 1, 894, DateTimeKind.Utc).AddTicks(2110), "5c4c162a-212a-4bba-b22f-c0391439e719", "db0db2b7-f8bb-4a2b-8c3f-ad12f24b5c3f", new DateTime(2023, 2, 8, 13, 4, 1, 894, DateTimeKind.Utc).AddTicks(2110), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 2, 8, 13, 4, 1, 894, DateTimeKind.Utc).AddTicks(2010), new DateTime(2023, 2, 8, 13, 4, 1, 894, DateTimeKind.Utc).AddTicks(2010) });
        }
    }
}
