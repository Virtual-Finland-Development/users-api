namespace VirtualFinland.UserAPI.Helpers.Configurations;

public interface IIdentityProviderConfig
{
    void LoadOpenIdConfig();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}