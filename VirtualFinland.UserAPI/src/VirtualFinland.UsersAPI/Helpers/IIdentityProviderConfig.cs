namespace VirtualFinland.UserAPI.Helpers;

public interface IIdentityProviderConfig
{
    void LoadOpenIDConfigUrl();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}