using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class SeedWarmUpUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("81846ac1-5155-4a60-a1c6-7632c51a5427"), new DateTime(2022, 11, 4, 12, 7, 28, 338, DateTimeKind.Utc).AddTicks(570), "83e480dd-af44-4bf9-a3a8-9db438b7a7f9", "78b3ace8-cca3-4d56-babe-1ecadf149ce0", new DateTime(2022, 11, 4, 12, 7, 28, 338, DateTimeKind.Utc).AddTicks(570), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CountryOfBirthISOCode", "Created", "DateOfBirth", "FirstName", "Gender", "ImmigrationDataConsent", "JobsDataConsent", "LastName", "Modified", "NationalityISOCode", "NativeLanguageISOCode", "OccupationISCOCode" },
                values: new object[] { new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"), null, null, new DateTime(2022, 11, 4, 12, 7, 28, 338, DateTimeKind.Utc).AddTicks(460), null, "WarmUpUser", null, false, false, "WarmUpUser", new DateTime(2022, 11, 4, 12, 7, 28, 338, DateTimeKind.Utc).AddTicks(460), null, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExternalIdentities",
                keyColumn: "Id",
                keyValue: new Guid("81846ac1-5155-4a60-a1c6-7632c51a5427"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"));
        }
    }
}
