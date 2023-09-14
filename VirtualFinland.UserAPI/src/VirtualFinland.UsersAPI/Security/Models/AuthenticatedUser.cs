namespace VirtualFinland.UserAPI.Security.Models;

public class AuthenticatedUser
{
    public AuthenticatedUser(Guid personId, string issuer, string audience)
    {
        PersonId = personId;
        Issuer = issuer;
        Audience = audience;
    }

    public AuthenticatedUser(string personId, string issuer, string audience)
    {
        PersonId = Guid.Parse(personId);
        Issuer = issuer;
        Audience = audience;
    }

    public Guid PersonId { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
}