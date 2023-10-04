namespace VirtualFinland.UserAPI.Security.Models;

public interface IRequestAuthenticationCandinate
{
    string IdentityId { get; set; }
    string Issuer { get; set; }
    string Audience { get; set; }
    string TraceId { get; set; }
}

public class RequestAuthenticationCandinate : IRequestAuthenticationCandinate
{
    public string IdentityId { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string TraceId { get; set; } = default!;
}