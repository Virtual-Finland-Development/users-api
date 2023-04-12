using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class UpdateCertificationTypeToEscoUri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("e870bcad-b6f2-4dfa-b7c8-c5c9595b7a26"));

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Certifications");

            migrationBuilder.AddColumn<string>(
                name: "EscoUri",
                table: "Certifications",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("5fecb1a4-0682-4246-95c1-7b3192f28692"), new DateTime(2023, 4, 1, 6, 39, 38, 67, DateTimeKind.Utc).AddTicks(3220), "0aa52102-b7d8-4abc-86d2-39bcd3916cf7", "2c0c4e0e-fc7f-4545-a10e-7710d8f95ba0", new DateTime(2023, 4, 1, 6, 39, 38, 67, DateTimeKind.Utc).AddTicks(3220), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 4, 1, 6, 39, 38, 67, DateTimeKind.Utc).AddTicks(3090), new DateTime(2023, 4, 1, 6, 39, 38, 67, DateTimeKind.Utc).AddTicks(3090) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("5fecb1a4-0682-4246-95c1-7b3192f28692"));

            migrationBuilder.DropColumn(
                name: "EscoUri",
                table: "Certifications");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Certifications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

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
    }
}
