using Microsoft.EntityFrameworkCore.Migrations;
using VirtualFinland.UserAPI.Models.UsersDatabase;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class AudibleMetadataFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "WorkPreferences",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Skills",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Persons",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "PersonAdditionalInformation",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Permits",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Occupations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Languages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Educations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<AuditableMetadata>(
                name: "Metadata",
                table: "Certifications",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "WorkPreferences");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "PersonAdditionalInformation");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Occupations");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Certifications");
        }
    }
}
