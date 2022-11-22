using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class GenderAndAddressDefinition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("1bcf54d5-7d87-4d79-973d-9fec4e2b626b"));

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("981cecb2-d916-49e7-b194-a9d0e81bf1b2"), new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(4050), "1766fb5f-c867-45e3-90ee-9b6fba5d2d80", "20ca085a-64e1-4787-ae5f-1a82049cb941", new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(4050), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Gender", "Modified" },
                values: new object[] { new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(3620), "0", new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(3620) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("981cecb2-d916-49e7-b194-a9d0e81bf1b2"));

            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("1bcf54d5-7d87-4d79-973d-9fec4e2b626b"), new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5420), "709e02eb-9215-4665-890b-9b508e9cc909", "3ab418ba-8bd0-4072-b74e-33b8eed26d9b", new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5420), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Gender", "Modified" },
                values: new object[] { new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5300), null, new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5300) });
        }
    }
}
