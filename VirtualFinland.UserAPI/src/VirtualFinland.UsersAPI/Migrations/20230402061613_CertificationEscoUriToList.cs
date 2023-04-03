using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class CertificationEscoUriToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("5fecb1a4-0682-4246-95c1-7b3192f28692"));

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("5346f52e-9927-436a-b515-f566c226b853"), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), "31098ce8-537e-4da3-b18d-3a4e0a34e900", "95b22fff-872b-4929-87e8-4db8509b61f3", new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                columns: new[] { "Created", "Modified" },
                values: new object[] { new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("5346f52e-9927-436a-b515-f566c226b853"));

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
    }
}
