using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class IndexAndCompositeKeyForOccupations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("8314a11f-6954-45e0-802e-e45d891ffe3f"));

            migrationBuilder.AlterColumn<string>(
                name: "EscoUri",
                table: "Occupations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EscoCode",
                table: "Occupations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Occupations_UserId_EscoUri_EscoCode",
                table: "Occupations",
                columns: new[] { "UserId", "EscoUri", "EscoCode" });

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("1627a486-b0ce-43c9-af3d-3587a4ea3c71"), new DateTime(2023, 1, 12, 10, 45, 23, 907, DateTimeKind.Utc).AddTicks(8510), "aa8b2fd9-f533-4761-8b0d-ab694c2de905", "cb1eb8ad-b1bc-492c-ad7a-bd56576b633d", new DateTime(2023, 1, 12, 10, 45, 23, 907, DateTimeKind.Utc).AddTicks(8510), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 1, 12, 10, 45, 23, 907, DateTimeKind.Utc).AddTicks(6740), new DateTime(2023, 1, 12, 10, 45, 23, 907, DateTimeKind.Utc).AddTicks(6740) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Occupations_UserId_EscoUri_EscoCode",
                table: "Occupations");

            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("1627a486-b0ce-43c9-af3d-3587a4ea3c71"));

            migrationBuilder.AlterColumn<string>(
                name: "EscoUri",
                table: "Occupations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EscoCode",
                table: "Occupations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);

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
        }
    }
}
