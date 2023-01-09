using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class OccupationUserRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Occupations_Users_UserId",
                table: "Occupations");

            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("1e394aa4-1e0a-4f38-86e3-ee95e84e53fd"));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Occupations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("8314a11f-6954-45e0-802e-e45d891ffe3f"), new DateTime(2023, 1, 5, 10, 45, 17, 453, DateTimeKind.Utc).AddTicks(4270), "6992e432-99f0-434e-b0a9-b68ef31df802", "4c8727e5-0b70-445c-a95a-873c1d6de4a2", new DateTime(2023, 1, 5, 10, 45, 17, 453, DateTimeKind.Utc).AddTicks(4270), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 1, 5, 10, 45, 17, 453, DateTimeKind.Utc).AddTicks(2980), new DateTime(2023, 1, 5, 10, 45, 17, 453, DateTimeKind.Utc).AddTicks(2980) });

            migrationBuilder.AddForeignKey(
                name: "FK_Occupations_Users_UserId",
                table: "Occupations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Occupations_Users_UserId",
                table: "Occupations");

            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("8314a11f-6954-45e0-802e-e45d891ffe3f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Occupations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("1e394aa4-1e0a-4f38-86e3-ee95e84e53fd"), new DateTime(2023, 1, 3, 7, 47, 58, 17, DateTimeKind.Utc).AddTicks(520), "bc32a78c-5c0a-4912-9587-35d3977debe2", "c9ead4c6-3d8b-47e1-aeb2-602124828743", new DateTime(2023, 1, 3, 7, 47, 58, 17, DateTimeKind.Utc).AddTicks(530), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 1, 3, 7, 47, 58, 16, DateTimeKind.Utc).AddTicks(9090), new DateTime(2023, 1, 3, 7, 47, 58, 16, DateTimeKind.Utc).AddTicks(9090) });

            migrationBuilder.AddForeignKey(
                name: "FK_Occupations_Users_UserId",
                table: "Occupations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
