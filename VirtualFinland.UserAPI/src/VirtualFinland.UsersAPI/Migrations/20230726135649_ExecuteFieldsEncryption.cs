﻿using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.UserAPI.Helpers;
using VirtualFinland.UserAPI.Helpers.Configurations;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    public partial class ExecuteFieldsEncryption : Migration
    {
        private readonly Dictionary<string, string[]> _encryptFields = new()
        {
            {"Persons", new[] {"GivenName", "LastName", "Email", "PhoneNumber", "ResidencyCode" }},
            {"PersonAdditionalInformation", new[] {
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
        }}};

        protected override async void Up(MigrationBuilder migrationBuilder)
        {
            // Skip if running in production environment (fail-safe)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return;
            }

            var access = await GetDbAccess();
            using var connection = access.Item1;

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
                        // Get GUID Id and field value from the result set
                        try
                        {
                            var id = result.GetGuid(0);
                            var fieldValue = result.GetString(1); // Encoding.UTF8.GetBytes(result.GetString(1));
                            var encryptedValue = access.Item2.Encrypt(fieldValue); // Convert.ToBase64String(access.Item2.Encrypt(fieldValue));
                            migrationBuilder.Sql($"UPDATE \"{table.Key}\" SET \"{field}\" = '{encryptedValue}' WHERE \"Id\" = '{id}'");
                        }
                        catch (Exception)
                        {
                            // Pass
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
            using var connection = access.Item1;

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
                        try
                        {
                            // Get GUID Id and field value from the result set
                            var id = result.GetGuid(0);
                            var fieldValue = result.GetString(1); //Encoding.UTF8.GetBytes(result.GetString(1));
                            var decryptedValue = access.Item2.Decrypt(fieldValue); // Encoding.UTF8.GetString(access.Item2.Decrypt(fieldValue)).Trim('\0');
                            migrationBuilder.Sql($"UPDATE \"{table.Key}\" SET \"{field}\" = '{decryptedValue}' WHERE \"Id\" = '{id}'");
                        }
                        catch (Exception)
                        {
                            // Pass
                        }
                    }
                }
            }
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

            return Tuple.Create(new UsersDbContext(contextOptions, secrets, auditInterceptor).Database.GetDbConnection(), cryptor);
        }
    }
}
