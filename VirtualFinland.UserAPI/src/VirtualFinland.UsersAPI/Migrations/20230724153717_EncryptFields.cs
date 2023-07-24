using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.Migrations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers.Configurations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class EncryptFields : Migration
    {

        protected override async void Up(MigrationBuilder migrationBuilder)
        {
            var access = await GetDbAccess();
            using (var connection = access.Item1)
            {
                // Open the connection
                connection.Open();


                using var command = connection.CreateCommand();
                command.CommandText = "SELECT \"Id\", \"Email\" FROM \"Persons\"";

                // Execute the SQL query and process the results
                using var result = command.ExecuteReader();

                while (result.Read())
                {
                    // Get GUID Id and email from the result set
                    var id = result.GetGuid(0);
                    var email = Encoding.UTF8.GetBytes(result.GetString(1));
                    var encryptedEmail = Convert.ToBase64String(access.Item2.Encrypt(email));
                    migrationBuilder.Sql($"UPDATE \"Persons\" SET \"Email\" = '{encryptedEmail}' WHERE \"Id\" = '{id}'");
                }
            }

        }

        protected override async void Down(MigrationBuilder migrationBuilder)
        {
            var access = await GetDbAccess();
            using (var connection = access.Item1)
            {
                // Open the connection
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Email FROM Persons";

                // Execute the SQL query and process the results
                using var result = command.ExecuteReader();

                while (result.Read())
                {
                    // Get GUID Id and email from the result set
                    var id = result.GetGuid(0);
                    var email = Encoding.UTF8.GetBytes(result.GetString(1));

                    var decryptedEmail = Encoding.UTF8.GetString(access.Item2.Decrypt(email)).Trim('\0');
                    migrationBuilder.Sql($"UPDATE \"Persons\" SET \"Email\" = '{decryptedEmail}' WHERE \"Id\" = '{id}'");
                }
            }
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
