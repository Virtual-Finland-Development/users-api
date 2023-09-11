using System.Text.Json;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Terms of Service Update
/// </summary>
public class TermsOfServiceUpdateAction : IAdminAppAction
{
    public async Task Execute(UsersDbContext dataContext, string? data)
    {
        // Parse payload
        var inputString = ReadActionInput(data);
        var payload = JsonSerializer.Deserialize<TermsOfServicesUpdatePayload>(inputString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
            ?? throw new ArgumentException("Invalid JSON payload");

        // Fetch existing terms of services
        var existingTermsOfServices = await dataContext.TermsOfServices.ToListAsync();

        // Prepare database actions
        foreach (var tossable in payload.TermsOfServices)
        {
            var existingTosItem = existingTermsOfServices.FirstOrDefault(tos => tos.Version == tossable.Version);
            if (existingTosItem == null)
            {
                // Insert
                var termsOfService = new TermsOfService
                {
                    Url = tossable.Url,
                    Description = tossable.Description,
                    Version = tossable.Version,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                };
                dataContext.TermsOfServices.Add(termsOfService);
            }
            else
            {
                if (tossable.Action == TermsOfServiceUpdateItem.SpecialActions.DELETE)
                {
                    // Delete
                    dataContext.TermsOfServices.Remove(existingTosItem);
                }
                else
                {
                    // Update
                    existingTosItem.Url = tossable.Url;
                    existingTosItem.Description = tossable.Description;
                    existingTosItem.Modified = DateTime.UtcNow;
                }
            }
        }

        // Apply changes
        await dataContext.SaveChangesAsync();
    }

    private static string ReadActionInput(string? data)
    {
        if (data != null)
        {
            return data;
        }

        // Read the built-in terms of service default
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException();
        string archiveFolder = Path.Combine(currentDirectory, "terms-of-services.json");

        return File.ReadAllText(archiveFolder);
    }
}