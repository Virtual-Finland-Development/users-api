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
            var encryptionKey = "12345678901234567890123456789012";
            var encryptionIV = "1234567890123456";
            var secrets = new DatabaseEncryptionSecrets(encryptionKey, encryptionIV);
            var provider = new AesProvider(secrets.EncryptionKey, secrets.EncryptionIV);

            AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();

            var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
                ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
                : null;
            var dbConnectionString = databaseSecret ?? "Host=localhost;Database=postgres;Username=postgres;Password=example";
            var contextOptions = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            using (var connection = new UsersDbContext(contextOptions, secrets).Database.GetDbConnection())
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

                    var encryptedEmail = provider.Encrypt(email);
                    migrationBuilder.Sql($"UPDATE Persons SET Email = '{encryptedEmail}' WHERE Id = {id}");
                }
            }
        }

        protected override async void Down(MigrationBuilder migrationBuilder)
        {
            var encryptionKey = "12345678901234567890123456789012";
            var encryptionIV = "1234567890123456";
            var secrets = new DatabaseEncryptionSecrets(encryptionKey, encryptionIV);
            var provider = new AesProvider(secrets.EncryptionKey, secrets.EncryptionIV);

            AwsConfigurationManager awsConfigurationManager = new AwsConfigurationManager();

            var databaseSecret = Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME") != null
                ? await awsConfigurationManager.GetSecretString(Environment.GetEnvironmentVariable("DB_CONNECTION_SECRET_NAME"))
                : null;
            var dbConnectionString = databaseSecret ?? "Host=localhost;Database=postgres;Username=postgres;Password=example";
            var contextOptions = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            using (var connection = new UsersDbContext(contextOptions, secrets).Database.GetDbConnection())
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

                    var encryptedEmail = provider.Decrypt(email);
                    migrationBuilder.Sql($"UPDATE Persons SET Email = '{encryptedEmail}' WHERE Id = {id}");
                }
            }
        }
    }
}
