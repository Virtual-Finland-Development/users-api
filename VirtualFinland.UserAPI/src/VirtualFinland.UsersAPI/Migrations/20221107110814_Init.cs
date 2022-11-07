using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Issuer = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    JobTitles = table.Column<List<string>>(type: "text[]", nullable: true),
                    Regions = table.Column<List<string>>(type: "text[]", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ImmigrationDataConsent = table.Column<bool>(type: "boolean", nullable: false),
                    JobsDataConsent = table.Column<bool>(type: "boolean", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    CountryOfBirthCode = table.Column<string>(type: "text", nullable: true),
                    NativeLanguageCode = table.Column<string>(type: "text", nullable: true),
                    OccupationCode = table.Column<string>(type: "text", nullable: true),
                    CitizenshipCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("e8e1b280-71ad-4018-acc5-e161c975b39d"), new DateTime(2022, 11, 7, 11, 8, 14, 777, DateTimeKind.Utc).AddTicks(1500), "d5ec2ed8-1ded-4457-83ac-4ad05909cb24", "88973016-baf2-4eb8-8a1c-67463581d066", new DateTime(2022, 11, 7, 11, 8, 14, 777, DateTimeKind.Utc).AddTicks(1500), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CitizenshipCode", "CountryOfBirthCode", "Created", "DateOfBirth", "FirstName", "Gender", "ImmigrationDataConsent", "JobsDataConsent", "LastName", "Modified", "NativeLanguageCode", "OccupationCode" },
                values: new object[] { new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"), null, null, null, new DateTime(2022, 11, 7, 11, 8, 14, 777, DateTimeKind.Utc).AddTicks(1400), null, "WarmUpUser", null, false, false, "WarmUpUser", new DateTime(2022, 11, 7, 11, 8, 14, 777, DateTimeKind.Utc).AddTicks(1400), null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalIdentities");

            migrationBuilder.DropTable(
                name: "SearchProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
