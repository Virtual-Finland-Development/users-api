using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class AddAudienceToExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Audiences",
                table: "ExternalIdentities",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""ExternalIdentities"" SET ""Audiences"" = '6fa88191-477e-4082-a119-e1e3ad09b7be' WHERE ""Audiences"" IS NULL AND ""Issuer"" = 'https://login.iam.qa.sinuna.fi';
                UPDATE ""ExternalIdentities"" SET ""Audiences"" = 'e6a5a645-0cf6-48a1-9f08-3d72be3aceaf' WHERE ""Audiences"" IS NULL AND ""Issuer"" = 'https://login.testbed.fi';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Audiences",
                table: "ExternalIdentities");
        }
    }
}
