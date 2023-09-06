using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class Terms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermsOfServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsOfServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonTermsOfService",
                columns: table => new
                {
                    PersonsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermsOfServicesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonTermsOfService", x => new { x.PersonsId, x.TermsOfServicesId });
                    table.ForeignKey(
                        name: "FK_PersonTermsOfService_Persons_PersonsId",
                        column: x => x.PersonsId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonTermsOfService_TermsOfServices_TermsOfServicesId",
                        column: x => x.TermsOfServicesId,
                        principalTable: "TermsOfServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonTermsOfService_TermsOfServicesId",
                table: "PersonTermsOfService",
                column: "TermsOfServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_TermsOfServices_Version",
                table: "TermsOfServices",
                column: "Version",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonTermsOfService");

            migrationBuilder.DropTable(
                name: "TermsOfServices");
        }
    }
}
