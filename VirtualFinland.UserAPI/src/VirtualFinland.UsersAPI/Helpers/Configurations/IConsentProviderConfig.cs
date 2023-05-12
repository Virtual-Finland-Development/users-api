using Microsoft.IdentityModel.Tokens;

namespace VirtualFinland.UserAPI.Helpers.Configurations;

public interface IConsentProviderConfig : IIdentityProviderConfig
{
    public JsonWebKey? GetKey(string kid);
    public string ConsentVerifyUrl { get; }
}