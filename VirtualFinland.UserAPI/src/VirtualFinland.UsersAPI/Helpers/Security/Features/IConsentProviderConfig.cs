using Microsoft.IdentityModel.Tokens;

namespace VirtualFinland.UserAPI.Helpers.Security.Features;

public interface IConsentProviderConfig
{
    void LoadPublicKeys();
    public JsonWebKey? GetKey(string kid);
    public string Issuer { get; }
    public string ConsentVerifyUrl { get; }
}