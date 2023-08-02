using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers.Security;

public class CryptoIdentityHelpers
{
    private readonly ICryptoUtility _cryptor;
    public CryptoIdentityHelpers(ICryptoUtility cryptor)
    {
        _cryptor = cryptor;
    }

    public string CreateNewPersonDataAccessKey()
    {
        return _cryptor.Hash(Guid.NewGuid().ToString());
    }

    public string EncryptExternalIdentityAccessKeyForPersonData(string personDataAccessKey, Guid userId, string issuer, string identityId)
    {
        var key = FormExternalIdentityAccessKeyForPersonData(userId, issuer, identityId);
        return _cryptor.Encrypt(personDataAccessKey, key);
    }
    public string DecryptExternalIdentityAccessKeyForPersonData(string externalIdentPersonKey, Guid userId, string issuer, string identityId)
    {
        return _cryptor.Decrypt(externalIdentPersonKey, FormExternalIdentityAccessKeyForPersonData(userId, issuer, identityId));
    }
    public string DecryptExternalIdentityAccessKeyForPersonData(ExternalIdentity externalIdentity, string identityId)
    {
        return DecryptExternalIdentityAccessKeyForPersonData(externalIdentity.KeyToPersonDataAccessKey, externalIdentity.UserId, externalIdentity.Issuer, identityId);
    }

    public string FormExternalIdentityAccessKeyForPersonData(Guid userId, string issuer, string identityId)
    {
        return $"{userId}::{issuer}::{identityId}";
    }
}