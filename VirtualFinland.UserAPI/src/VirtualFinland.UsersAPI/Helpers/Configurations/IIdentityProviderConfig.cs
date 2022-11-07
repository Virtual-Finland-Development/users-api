namespace VirtualFinland.UserAPI.Helpers.Configurations;

public interface IIdentityProviderConfig
{
    void LoadOpenIdConfigUrl();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}