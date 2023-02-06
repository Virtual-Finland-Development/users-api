using System;
using System.Collections.Generic;
using Amazon.SecretsManager.Model.Internal.MarshallTransformations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty database for this new "initial" migration
            migrationBuilder.Sql(@"DELETE FROM ""__EFMigrationsHistory""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Certifications""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Educations""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Languages""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Occupations""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Permits""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""SearchProfiles""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Skills""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""WorkPreferences""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""ExternalIdentities""", true);
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Users""", true);

            migrationBuilder.CreateTable(
                name: "ExternalIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Issuer = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GivenName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ResidencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    JobTitles = table.Column<List<string>>(type: "text[]", nullable: true),
                    Regions = table.Column<List<string>>(type: "text[]", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InstitutionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certifications_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Educations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EducationLevelCode = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    EducationFieldCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    GraduationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InstitutionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Educations_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscoUri = table.Column<string>(type: "text", nullable: true),
                    LanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CerfCode = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Languages_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Occupations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NaceCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    EscoUri = table.Column<string>(type: "text", nullable: true),
                    EscoCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Employer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WorkMonths = table.Column<int>(type: "integer", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occupations_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permits_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonAdditionalInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreetAddress = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    City = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Country = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    CountryOfBirthCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NativeLanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OccupationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CitizenshipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAdditionalInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonAdditionalInformation_Persons_Id",
                        column: x => x.Id,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscoUri = table.Column<string>(type: "text", nullable: true),
                    SkillLevelEnum = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredRegionCode = table.Column<string>(type: "text", nullable: false),
                    PreferredMunicipalityCode = table.Column<string>(type: "text", nullable: false),
                    EmploymentTypeCode = table.Column<string>(type: "text", nullable: true),
                    WorkingTimeCode = table.Column<string>(type: "text", nullable: true),
                    WorkingLanguageEnum = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPreferences_Persons_Id",
                        column: x => x.Id,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExternalIdentities",
                columns: new[] { "Id", "Created", "IdentityId", "Issuer", "Modified", "UserId" },
                values: new object[] { new Guid("55829074-cc71-4b7d-942d-62e12ec2a2d2"), new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3380), "3429e94e-e55e-4a5c-b5d5-62a11d5b0401", "9714f04a-1184-4ed7-a53d-ef12e5a7bcd8", new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3380), new Guid("5a8af4b4-8cb4-44ac-8291-010614601719") });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Created", "Email", "GivenName", "LastName", "Modified", "PhoneNumber", "ResidencyCode" },
                values: new object[] { new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"), new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3290), null, "WarmUpUserGivenName", "WarmUpUserLastName", new DateTime(2023, 1, 30, 17, 27, 44, 270, DateTimeKind.Utc).AddTicks(3290), null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_PersonId",
                table: "Certifications",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Educations_PersonId",
                table: "Educations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_PersonId",
                table: "Languages",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Occupations_PersonId",
                table: "Occupations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Permits_PersonId",
                table: "Permits",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_PersonId",
                table: "Skills",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Educations");

            migrationBuilder.DropTable(
                name: "ExternalIdentities");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Occupations");

            migrationBuilder.DropTable(
                name: "Permits");

            migrationBuilder.DropTable(
                name: "PersonAdditionalInformation");

            migrationBuilder.DropTable(
                name: "SearchProfiles");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "WorkPreferences");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
