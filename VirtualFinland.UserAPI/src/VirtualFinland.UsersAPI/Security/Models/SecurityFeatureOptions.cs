namespace VirtualFinland.UserAPI.Security.Models;

public class SecurityFeatureOptions
{
    public string? OpenIdConfigurationUrl { get; set; }
    public string? AuthorizationJwksJsonUrl { get; set; }
    public string? Issuer { get; set; }
    public bool IsEnabled { get; set; }
}