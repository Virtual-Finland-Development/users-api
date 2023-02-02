using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class AllowWorkPreferenceRegionsAndMunicipalitiesToBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("55829074-cc71-4b7d-942d-62e12ec2a2d2"));

            migrationBuilder.AlterColumn<string>(
                name: "PreferredRegionCode",
                table: "WorkPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMunicipalityCode",
                table: "WorkPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("618c827c-9697-4c01-8f46-04f67f9ce9b5"));

            migrationBuilder.AlterColumn<string>(
                name: "PreferredRegionCode",
                table: "WorkPreferences",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreferredMunicipalityCode",
                table: "WorkPreferences",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("55829074-cc71-4b7d-942d-62e12ec2a2d2"), new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3380), "3429e94e-e55e-4a5c-b5d5-62a11d5b0401", "9714f04a-1184-4ed7-a53d-ef12e5a7bcd8", new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3380), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3290), new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3290) });
        }
    }
}
