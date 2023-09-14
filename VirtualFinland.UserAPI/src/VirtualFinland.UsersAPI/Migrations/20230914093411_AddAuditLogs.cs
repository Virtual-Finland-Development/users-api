using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    KeyValues = table.Column<string>(type: "text", nullable: false),
                    ChangedColumns = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"LC_TRIGGER_AFTER_UPDATE_PERSON\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PERSON$\r\nBEGIN\r\n  INSERT INTO \"AuditLogs\" (\"TableName\", \"Action\", \"KeyValues\", \"ChangedColumns\", \"EventDate\") SELECT 'Persons', \r\n  'Update', \r\n  '[Id]', \r\n  '[...]', \r\n  NEW.\"Modified\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PERSON$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PERSON AFTER UPDATE\r\nON \"Persons\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"LC_TRIGGER_AFTER_UPDATE_PERSON\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"LC_TRIGGER_AFTER_UPDATE_PERSON\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
