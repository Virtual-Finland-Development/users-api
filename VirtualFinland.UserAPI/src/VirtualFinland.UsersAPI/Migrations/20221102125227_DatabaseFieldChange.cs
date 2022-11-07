using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class DatabaseFieldChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OccupationISCOCode",
                table: "Users",
                newName: "OccupationCode");

            migrationBuilder.RenameColumn(
                name: "NativeLanguageISOCode",
                table: "Users",
                newName: "NativeLanguageCode");

            migrationBuilder.RenameColumn(
                name: "NationalityISOCode",
                table: "Users",
                newName: "CountryOfBirthCode");

            migrationBuilder.RenameColumn(
                name: "CountryOfBirthISOCode",
                table: "Users",
                newName: "CitizenshipCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OccupationCode",
                table: "Users",
                newName: "OccupationISCOCode");

            migrationBuilder.RenameColumn(
                name: "NativeLanguageCode",
                table: "Users",
                newName: "NativeLanguageISOCode");

            migrationBuilder.RenameColumn(
                name: "CountryOfBirthCode",
                table: "Users",
                newName: "NationalityISOCode");

            migrationBuilder.RenameColumn(
                name: "CitizenshipCode",
                table: "Users",
                newName: "CountryOfBirthISOCode");
        }
    }
}
