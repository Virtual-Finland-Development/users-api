using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class ChangeWorkingTimeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("07621d0e-085a-414e-a2a2-2302ce7e0f8b"));

            migrationBuilder.AlterColumn<string>(
                name: "WorkingTimeEnum",
                table: "WorkPreferences",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EscoCode",
                table: "Occupations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("1e394aa4-1e0a-4f38-86e3-ee95e84e53fd"));

            migrationBuilder.AlterColumn<int>(
                name: "WorkingTimeEnum",
                table: "WorkPreferences",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EscoCode",
                table: "Occupations",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("07621d0e-085a-414e-a2a2-2302ce7e0f8b"), new DateTime(2022, 12, 28, 17, 57, 53, 791, DateTimeKind.Utc).AddTicks(5240), "a7c0db2e-a03f-4cf6-a5f2-290415bcb7fa", "7a2de561-7f7e-4e3c-bc02-68b261f6bd99", new DateTime(2022, 12, 28, 17, 57, 53, 791, DateTimeKind.Utc).AddTicks(5240), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2022, 12, 28, 17, 57, 53, 790, DateTimeKind.Utc).AddTicks(7550), new DateTime(2022, 12, 28, 17, 57, 53, 790, DateTimeKind.Utc).AddTicks(7550) });
        }
    }
}
