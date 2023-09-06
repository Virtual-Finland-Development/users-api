using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class TermsOfService : Migration
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
                name: "PersonTermsOfServiceAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermsOfServiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonTermsOfServiceAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonTermsOfServiceAgreements_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonTermsOfServiceAgreements_TermsOfServices_TermsOfServi~",
                        column: x => x.TermsOfServiceId,
                        principalTable: "TermsOfServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonTermsOfServiceAgreements_PersonId",
                table: "PersonTermsOfServiceAgreements",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonTermsOfServiceAgreements_TermsOfServiceId",
                table: "PersonTermsOfServiceAgreements",
                column: "TermsOfServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TermsOfServices_Version",
                table: "TermsOfServices",
                column: "Version",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonTermsOfServiceAgreements");

            migrationBuilder.DropTable(
                name: "TermsOfServices");
        }
    }
}
