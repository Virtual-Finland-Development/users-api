using Microsoft.EntityFrameworkCore.Diagnostics;
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
        if (instance is IEncrypted)
        {
            // Loop through the properties of the entity
            foreach (var property in instance.GetType().GetProperties())
            {
                // If the property is of type string, encrypt it
                if (property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(instance)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        property.SetValue(instance, _cryptor.Decrypt(value));
                    }
                }
            }
        }

        return instance;
    }
}
