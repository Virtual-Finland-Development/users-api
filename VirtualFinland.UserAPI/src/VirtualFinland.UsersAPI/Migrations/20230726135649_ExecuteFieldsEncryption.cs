using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;
using VirtualFinland.UserAPI.Helpers.Security;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class ExecuteFieldsEncryption : Migration
    {
        private readonly Dictionary<string, string[]> _encryptFields = new()
        {
            {"Persons", new[] {"GivenName", "LastName", "Email", "PhoneNumber", "ResidencyCode" }},
            /* {"PersonAdditionalInformation", new[] {
                "StreetAddress",
                "ZipCode",
                "City",
                "Country",
                "Gender",
                "DateOfBirth",
                "CountryOfBirthCode",
                "NativeLanguageCode",
                "OccupationCode",
                "CitizenshipCode"
            }} */
            {"ExternalIdentities", new[] {"IdentityId"}}
        };

        protected override async void Up(MigrationBuilder migrationBuilder)
        {
            // Skip if running in production environment (fail-safe)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return;
            }

            var dbAccess = await GetDbAccess();
            using var connection = dbAccess.Item1;

            foreach (var table in _encryptFields)
            {
                foreach (var field in table.Value)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = table.Key switch
                    {
                        "Persons" => $"SELECT \"Id\", \"Id\" AS \"UserId\", \"{field}\" FROM \"{table.Key}\"",
                        //"PersonAdditionalInformation" => $"SELECT \"Id\", \"PersonId\", \"{field}\" FROM \"{table.Key}\"",
                        "ExternalIdentities" => $"SELECT \"Id\", \"UserId\", \"{field}\" FROM \"{table.Key}\"",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    // Execute the SQL query and process the results
                    using var result = command.ExecuteReader();

                    var results = new List<Tuple<string, string, string>>();
                    while (result.Read())
                    {
                        // Get GUID Id and field value from the result set
                        try
                        {
                            var id = result.GetGuid(0).ToString();
                            var userId = result.GetGuid(1).ToString();
                            var fieldValue = result.GetString(2);

                            results.Add(Tuple.Create(id, userId, fieldValue));
                        }
                        catch (Exception)
                        {
                            // Pass
                        }
                    }

                    // Close the command
                    result.Close();

                    for (var i = 0; i < results.Count; i++)
                    {
                        var (id, userId, fieldValue) = results[i];
                        var (secretKey, identityHash) = ResolveItemSecretKey(dbAccess, userId);

                        // Get GUID Id and field value from the result set
                        try
                        {
                            var secretKeyResult = ResolveItemSecretKey(dbAccess, userId.ToString());
                            migrationBuilder.Sql($"UPDATE \"ExternalIdentities\" SET \"IdentityHash\" = '{secretKeyResult.Item2}' WHERE \"UserId\" = '{userId}'"); // @TODO: Use external access instead

                            var encryptedValue = dbAccess.Item2.Encrypt(fieldValue, secretKeyResult.Item1);
                            migrationBuilder.Sql($"UPDATE \"{table.Key}\" SET \"{field}\" = '{encryptedValue}' WHERE \"Id\" = '{id}'");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            migrationBuilder.AlterColumn<string>(
                name: "IdentityHash",
                table: "ExternalIdentities",
                nullable: false,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Skip if running in production environment (fail-safe)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return;
            }

            migrationBuilder.AlterColumn<string>(
                name: "IdentityHash",
                table: "ExternalIdentities",
                nullable: true,
                oldNullable: false);

            Console.WriteLine("---> Reverting the encrypted fields is not possible without knowing the external identities, which this migration does not know.");
        }

        private static async Task<Tuple<DbConnection, CryptoUtility>> GetDbAccess()
        {
            AwsConfigurationManager awsConfigurationManager = new();
            var builder = WebApplication.CreateBuilder();

            var encryptionKeySecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_ENCRYPTION_KEY_SECRET_NAME");
            var encryptionKey = encryptionKeySecret ?? builder.Configuration.GetValue<string>("Database:EncryptionKey");
            var encryptionIVSecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_ENCRYPTION_IV_SECRET_NAME");
            var encryptionIV = encryptionKeySecret ?? builder.Configuration.GetValue<string>("Database:EncryptionIV");

            var secrets = new DatabaseEncryptionSecrets(encryptionKey, encryptionIV);
            var cryptor = new CryptoUtility(secrets);

            var databaseSecret = await awsConfigurationManager.GetSecretByEnvironmentValueName("DB_CONNECTION_SECRET_NAME");
            var dbConnectionString = databaseSecret ?? builder.Configuration.GetConnectionString("DefaultConnection");

            var contextOptions = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var auditInterceptor = new AuditInterceptor(loggerFactory.CreateLogger<IAuditInterceptor>());


            // Open the connection
            var connection = new UsersDbContext(contextOptions, secrets, auditInterceptor).Database.GetDbConnection();
            connection.Open();

            return Tuple.Create(connection, cryptor);
        }


        private static Tuple<string, string> ResolveItemSecretKey(Tuple<DbConnection, CryptoUtility> dbAccess, string userId)
        {
            using var command = dbAccess.Item1.CreateCommand();
            command.CommandText = $"SELECT \"IdentityId\" FROM \"ExternalIdentities\" WHERE \"UserId\" = '{userId}'"; // @TODO: Use external access instead

            // Execute the SQL query and process the results
            using var result = command.ExecuteReader();

            while (result.Read())
            {
                // Get GUID Id and field value from the result set
                try
                {
                    var secretKey = result.GetString(0);
                    var identityHash = dbAccess.Item2.Hash(secretKey);
                    result.Close();
                    return Tuple.Create(secretKey, identityHash);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            throw new ArgumentException($"Could not resolve secret key for userId {userId}");
        }
    }
}
