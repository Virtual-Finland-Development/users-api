using System.Text.Json;
using VirtualFinland.UserAPI.Data;
using VirtualFinland.AdminFunction.AdminApp.Models.TermsOfServices;
using VirtualFinland.UserAPI.Models.UsersDatabase;
using Microsoft.EntityFrameworkCore;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Terms of Service Update
/// </summary>
public class TermsOfServiceUpdateAction : IAdminAppAction
{
    public async Task Execute(UsersDbContext dataContext, string? data)
    {
        var inputString = data ?? throw new ArgumentNullException(nameof(data));
        var payload = JsonSerializer.Deserialize<TermsOfServicesUpdatePayload>(inputString) ?? throw new ArgumentException("Invalid JSON payload");

        // Existing tosses
        var existingTermsOfServices = await dataContext.TermsOfServices.ToListAsync();

        // Sync input terms of services with database
        var toBeAdded = new List<TermsOfServiceUpdateItem>();
        var toBeDeleted = new List<TermsOfService>();
        var toBeUpdated = new List<TermsOfServiceUpdateItem>();

        // Deletes
        foreach (var termsOfService in existingTermsOfServices)
        {
            var existingTosItem = payload.TermsOfServices.FirstOrDefault(tos => tos.Version == termsOfService.Version);
            if (existingTosItem == null)
            {
                dataContext.TermsOfServices.Remove(termsOfService);
            }
        }

        // Updates adn additions
        foreach (var payloadTosItem in payload.TermsOfServices)
        {
            var existingTosItem = existingTermsOfServices.FirstOrDefault(tos => tos.Version == payloadTosItem.Version);
            if (existingTosItem == null)
            {
                var termsOfService = new TermsOfService
                {
                    Url = payloadTosItem.Url,
                    Description = payloadTosItem.Description,
                    Version = payloadTosItem.Version
                };
                dataContext.TermsOfServices.Add(termsOfService);
            }
            else
            {
                existingTosItem.Url = payloadTosItem.Url;
                existingTosItem.Description = payloadTosItem.Description;
            }
        }

        // Apply changes
        await dataContext.SaveChangesAsync();
    }
}