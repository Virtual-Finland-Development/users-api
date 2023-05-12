using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class DropInitialSeeds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("5346f52e-9927-436a-b515-f566c226b853"));

            migrationBuilder.DeleteData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("5346f52e-9927-436a-b515-f566c226b853"), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), "31098ce8-537e-4da3-b18d-3a4e0a34e900", "95b22fff-872b-4929-87e8-4db8509b61f3", new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5580), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Created", "Email", "GivenName", "LastName", "Modified", "PhoneNumber", "ResidencyCode" },
                values: new object[] { new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"), new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470), null, "WarmUpUserGivenName", "WarmUpUserLastName", new DateTime(2023, 4, 2, 6, 16, 13, 703, DateTimeKind.Utc).AddTicks(5470), null, null });
        }
    }
}
