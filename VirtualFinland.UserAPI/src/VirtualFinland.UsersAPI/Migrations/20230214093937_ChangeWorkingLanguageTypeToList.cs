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

            migrationBuilder.AddColumn<string>(
                name: "WorkingLanguageCode",
                table: "WorkPreferences",
                type: "text",
                nullable: true);

            // Copy data from WorkingLanguageEnum to WorkingLanguageCode
            migrationBuilder.Sql(@"
                UPDATE ""WorkPreferences""
                SET ""WorkingLanguageCode"" = Array[""WorkingLanguageEnum""]
                WHERE ""WorkingLanguageEnum"" IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "WorkingLanguageEnum",
                table: "WorkPreferences");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("38acc6f7-45bf-4956-8793-046de6bc5826"), new DateTime(2023, 2, 14, 9, 39, 37, 747, DateTimeKind.Utc).AddTicks(9268), "57beee60-54ef-4f2c-87dd-e2ea1a84d1f6", "c90f971f-c08b-4524-aa37-59bd85e408a5", new DateTime(2023, 2, 14, 9, 39, 37, 747, DateTimeKind.Utc).AddTicks(9269), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 2, 14, 9, 39, 37, 747, DateTimeKind.Utc).AddTicks(9186), new DateTime(2023, 2, 14, 9, 39, 37, 747, DateTimeKind.Utc).AddTicks(9187) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("38acc6f7-45bf-4956-8793-046de6bc5826"));

            migrationBuilder.AddColumn<string>(
                name: "WorkingLanguageEnum",
                table: "WorkPreferences",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            // Copy data from WorkingLanguageCode to WorkingLanguageEnum
            migrationBuilder.Sql(@"
                UPDATE ""WorkPreferences""
                SET ""WorkingLanguageEnum"" = ""WorkingLanguageCode""[1]
                WHERE array_length(""WorkingLanguageCode"", 1) > 0;");

            migrationBuilder.DropColumn(
               name: "WorkingLanguageCode",
               table: "WorkPreferences");

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
