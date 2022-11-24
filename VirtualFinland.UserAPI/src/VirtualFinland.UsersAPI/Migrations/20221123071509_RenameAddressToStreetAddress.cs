using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class RenameAddressToStreetAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("981cecb2-d916-49e7-b194-a9d0e81bf1b2"));

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Users",
                newName: "StreetAddress");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("9ed28e82-6cb9-4c48-a27c-603c5a97221f"), new DateTime(2022, 11, 23, 7, 15, 9, 238, DateTimeKind.Utc).AddTicks(4460), "a8f51b88-af6b-4a95-b2d8-da62e0fb4f65", "15b205f4-98bc-491f-8616-ce66043ee7f6", new DateTime(2022, 11, 23, 7, 15, 9, 238, DateTimeKind.Utc).AddTicks(4460), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2022, 11, 23, 7, 15, 9, 238, DateTimeKind.Utc).AddTicks(4010), new DateTime(2022, 11, 23, 7, 15, 9, 238, DateTimeKind.Utc).AddTicks(4010) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("9ed28e82-6cb9-4c48-a27c-603c5a97221f"));

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "Users",
                newName: "Address");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("981cecb2-d916-49e7-b194-a9d0e81bf1b2"), new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(4050), "1766fb5f-c867-45e3-90ee-9b6fba5d2d80", "20ca085a-64e1-4787-ae5f-1a82049cb941", new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(4050), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(3620), new DateTime(2022, 11, 22, 7, 23, 44, 435, DateTimeKind.Utc).AddTicks(3620) });
        }
    }
}
