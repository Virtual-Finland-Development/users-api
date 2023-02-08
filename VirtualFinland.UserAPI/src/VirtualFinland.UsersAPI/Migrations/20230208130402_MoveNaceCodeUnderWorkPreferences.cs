using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class MoveNaceCodeUnderWorkPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("618c827c-9697-4c01-8f46-04f67f9ce9b5"));

            migrationBuilder.DropColumn(
                name: "NaceCode",
                table: "Occupations");

            migrationBuilder.AddColumn<string>(
                name: "NaceCode",
                table: "WorkPreferences",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("fb064d95-0733-4cf0-a836-0642a04fd46b"));

            migrationBuilder.DropColumn(
                name: "NaceCode",
                table: "WorkPreferences");

            migrationBuilder.AddColumn<string>(
                name: "NaceCode",
                table: "Occupations",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("618c827c-9697-4c01-8f46-04f67f9ce9b5"), new DateTime(2023, 2, 2, 6, 48, 20, 519, DateTimeKind.Utc).AddTicks(2260), "b29867f2-6480-40a5-8ca0-b2c8598b8c5e", "95786f49-562d-4227-886a-a98bf9b5b386", new DateTime(2023, 2, 2, 6, 48, 20, 519, DateTimeKind.Utc).AddTicks(2260), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 2, 2, 6, 48, 20, 519, DateTimeKind.Utc).AddTicks(2160), new DateTime(2023, 2, 2, 6, 48, 20, 519, DateTimeKind.Utc).AddTicks(2160) });
        }
    }
}
