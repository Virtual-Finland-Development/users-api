using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.Migrations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;

//
// A slow migration ONLY for development environments, remove before deploying to production
//
namespace VirtualFinland.UserAPI.Migrations
{
    public partial class EncryptFields : Migration
    {
        private readonly Dictionary<string, string[]> _encryptFields = new()
        {
            {"Persons", new[] {"GivenName", "LastName", "Email", "PhoneNumber", "ResidencyCode" }},
            //{"PersonAdditionalInformation", new[] {"Gender", "CountryOfBirthCode", "NativeLanguageCode", "OccupationCode", "CitizenshipCode" }}, // DateOfBirth
        };

        protected override async void Up(MigrationBuilder migrationBuilder)
        {
            // Skip if running in production environment (fail-safe)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return;
            }


            migrationBuilder.AlterColumn<string>(
                name: "ResidencyCode",
                table: "Persons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GivenName",
                table: "Persons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);


            var access = await GetDbAccess();
            using (var connection = access.Item1)
            {
                // Open the connection
                connection.Open();

                foreach (var table in _encryptFields)
                {
                    foreach (var field in table.Value)
                    {
                        using var command = connection.CreateCommand();
                        command.CommandText = $"SELECT \"Id\", \"{field}\" FROM \"{table.Key}\"";

                        // Execute the SQL query and process the results
                        using var result = command.ExecuteReader();

                        while (result.Read())
                        {
                            // Get GUID Id and email from the result set
                            var id = result.GetGuid(0);
                            var email = Encoding.UTF8.GetBytes(result.GetString(1));
                            var encryptedEmail = Convert.ToBase64String(access.Item2.Encrypt(email));
                            migrationBuilder.Sql($"UPDATE \"{table.Key}\" SET \"{field}\" = '{encryptedEmail}' WHERE \"Id\" = '{id}'");
                        }
                    }
                }
            }

        }

        protected override async void Down(MigrationBuilder migrationBuilder)
        {
            // Skip if running in production environment (fail-safe)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return;
            }

            var access = await GetDbAccess();
            using (var connection = access.Item1)
            {
                // Open the connection
                connection.Open();

                foreach (var table in _encryptFields)
                {
                    foreach (var field in table.Value)
                    {
                        using var command = connection.CreateCommand();
                        command.CommandText = $"SELECT \"Id\", \"{field}\" FROM \"{table.Key}\"";

                        // Execute the SQL query and process the results
                        using var result = command.ExecuteReader();

                        while (result.Read())
                        {
                            // Get GUID Id and email from the result set
                            var id = result.GetGuid(0);
                            var email = Encoding.UTF8.GetBytes(result.GetString(1));
                            var decryptedEmail = Encoding.UTF8.GetString(access.Item2.Decrypt(email)).Trim('\0');
                            migrationBuilder.Sql($"UPDATE \"{table.Key}\" SET \"{field}\" = '{decryptedEmail}' WHERE \"Id\" = '{id}'");
                        }
                    }
                }
            }

            migrationBuilder.AlterColumn<string>(
                name: "ResidencyCode",
                table: "Persons",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GivenName",
                table: "Persons",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        private async Task<Tuple<DbConnection, AesProvider>> GetDbAccess()
        {
            AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();
            var builder = WebApplication.CreateBuilder();

            var encryptionKeySecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_ENCRYPTION_KEY_SECRET_NAME");
            var encryptionKey = encryptionKeySecret ?? builder.Configuration.GetValue<string>("Database:EncryptionKey");
            var encryptionIVSecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_ENCRYPTION_IV_SECRET_NAME");
            var encryptionIV = encryptionKeySecret ?? builder.Configuration.GetValue<string>("Database:EncryptionIV");

            var secrets = new DatabaseEncryptionSecrets(encryptionKey, encryptionIV);
            var provider = new AesProvider(secrets.EncryptionKey, secrets.EncryptionIV);


            var databaseSecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_CONNECTION_SECRET_NAME");
            var dbConnectionString = databaseSecret ?? builder.Configuration.GetConnectionString("DefaultConnection");

            var contextOptions = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            return Tuple.Create(new UsersDbContext(contextOptions, secrets).Database.GetDbConnection(), provider);
        }
    }
}
