namespace VirtualFinland.UserAPI.Security.Models;

public class AuthenticatedUser : AuthenticationCandinate
{
    public Guid PersonId { get; set; }
}