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
    private readonly UsersDbContext _dataContext;
    public TermsOfServiceUpdateAction(UsersDbContext dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task Execute(string? data)
    {
        // Parse payload
        var inputString = ReadActionInput(data);
        var payload = JsonSerializer.Deserialize<TermsOfServicesUpdatePayload>(inputString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
            ?? throw new ArgumentException("Invalid JSON payload");

        // Fetch existing terms of services
        var existingTermsOfServices = await _dataContext.TermsOfServices.ToListAsync();

        // Prepare database actions
        foreach (var tossable in payload.TermsOfServices)
        {
            var existingTosItem = existingTermsOfServices.FirstOrDefault(tos => tos.Version == tossable.Version);
            if (existingTosItem == null)
            {
                if (tossable.Action == "DELETE")
                {
                    // No need to delete something that does not exist
                    continue;
                }

                // Insert
                var termsOfService = new TermsOfService
                {
                    Url = tossable.Url,
                    Description = tossable.Description,
                    Version = tossable.Version,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                };
                _dataContext.TermsOfServices.Add(termsOfService);
            }
            else
            {
                if (tossable.Action == "DELETE")
                {
                    // Delete
                    _dataContext.TermsOfServices.Remove(existingTosItem);
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
        await _dataContext.SaveChangesAsync();
    }

    private static string ReadActionInput(string? data)
    {
        if (data != null)
        {
            return data;
        }

        // Read the built-in terms of service default
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException();

        // Fix: lambda runtime is not in the same folder as the function code
        if (currentDirectory == "/var/runtime")
        {
            currentDirectory = "/var/task";
        }

        string archiveFolder = Path.Combine(currentDirectory, "terms-of-services.json");

        return File.ReadAllText(archiveFolder);
    }
}