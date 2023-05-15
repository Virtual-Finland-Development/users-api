namespace VirtualFinland.UserAPI.Helpers.Configurations;

public interface IIdentityProviderConfig
{
    Task LoadOpenIdConfig();
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
}