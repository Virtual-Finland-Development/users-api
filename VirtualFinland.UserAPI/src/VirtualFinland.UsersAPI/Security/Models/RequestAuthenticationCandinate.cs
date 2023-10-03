namespace VirtualFinland.UserAPI.Security.Models;

public class RequestAuthenticationCandinate
{
    public string IdentityId { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string TraceId { get; set; } = default!;
}