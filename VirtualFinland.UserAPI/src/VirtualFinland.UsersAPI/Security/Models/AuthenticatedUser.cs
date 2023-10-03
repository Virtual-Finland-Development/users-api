using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Security.Models;

public class AuthenticatedUser : JwtTokenResult
{
    public AuthenticatedUser(Person person, JwtTokenResult jwtTokenResult)
    {
        PersonId = person.Id;
        IdentityId = jwtTokenResult.IdentityId;
        Issuer = jwtTokenResult.Issuer;
        Audience = jwtTokenResult.Audience;
    }

    public Guid PersonId { get; set; } = default!;
}