using System.Text.Json;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Security.Models;

public class RequestAuthenticatedUser : RequestAuthenticationCandinate
{
    public RequestAuthenticatedUser()
    {
    }

    public RequestAuthenticatedUser(Guid personId)
    {
        PersonId = personId;
    }

    public RequestAuthenticatedUser(Person person, RequestAuthenticationCandinate requestAuthenticationCandinate)
    {
        PersonId = person.Id;
        IdentityId = requestAuthenticationCandinate.IdentityId;
        Issuer = requestAuthenticationCandinate.Issuer;
        Audience = requestAuthenticationCandinate.Audience;
    }

    public Guid PersonId { get; set; } = default!;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}