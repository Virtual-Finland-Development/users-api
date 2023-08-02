using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Helpers.Security;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers;

public interface IDecryptionInterceptor : IInterceptor
{
}

public class DecryptionInterceptor : IMaterializationInterceptor, IDecryptionInterceptor
{
    private readonly ICryptoUtility _cryptor;
    public DecryptionInterceptor(ICryptoUtility cryptor)
    {
        _cryptor = cryptor;
    }

    // <summary>
    // Decrypts the values of the properties of the entities that implement IEncrypted
    // </summary>
    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        var entityName = instance.GetType().Name;
        if (instance is IEncrypted item && !_cryptor.State.IsQueryKeyDisabled(entityName))
        {
            var secretKey = _cryptor.State.GetQueryKey(entityName) ?? item.DataAccessKey;

            // Loop through the properties of the entity
            foreach (var property in instance.GetType().GetProperties())
            {
                var encryptedAttribute = property.GetCustomAttribute<EncryptedAttribute>();
                if (encryptedAttribute == null)
                    continue;

                var value = property.GetValue(instance)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    property.SetValue(item, _cryptor.Decrypt(value, secretKey));
            }

            item.DataAccessKey = secretKey;
        }

        return instance;
    }
}
