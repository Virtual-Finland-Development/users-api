namespace VirtualFinland.UserAPI.Helpers;

public interface IIdentityProviderConfig
{
    void LoadOpenIdConfigUrl();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}