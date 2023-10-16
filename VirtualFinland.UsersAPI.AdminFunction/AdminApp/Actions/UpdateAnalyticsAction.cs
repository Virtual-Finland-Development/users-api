using System.Text.Json;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.AdminFunction.AdminApp.Actions;

/// <summary>
/// Sets up the database user for the application. 
/// The intented use is to allow the pulumi deployment process to create and manage the application level database user.
/// In live environments the script is called during a deployment (finishing phases) from a cloud function residing in the same virtual private cloud (VPC) as the database.
/// </summary>
public class UpdateAnalyticsAction : IAdminAppAction
{
    public async Task Execute(UsersDbContext dataContext, string? eventPayload)
    {
        if (eventPayload == null)
        {
            throw new ArgumentNullException(nameof(eventPayload));
        }
        var analyticsEvent = JsonSerializer.Deserialize<AnalyticsEvent>(eventPayload, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Invalid analytics event payload", nameof(eventPayload));



    }

    public record AnalyticsEvent(string Username);
}