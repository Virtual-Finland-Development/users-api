using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Security.Models;

public class RequestAuthenticatedUser : RequestAuthenticationCandinate
{
    public RequestAuthenticatedUser(Person person, RequestAuthenticationCandinate RequestAuthenticationCandinate)
    {
        PersonId = person.Id;
        IdentityId = RequestAuthenticationCandinate.IdentityId;
        Issuer = RequestAuthenticationCandinate.Issuer;
        Audience = RequestAuthenticationCandinate.Audience;
    }

    public Guid PersonId { get; set; } = default!;
}