using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers;

public interface IEncryptionInterceptor : IInterceptor
{
}

public class EncryptionInterceptor : SaveChangesInterceptor, IEncryptionInterceptor
{
    private readonly ICryptoUtility _cryptor;
    public EncryptionInterceptor(ICryptoUtility cryptor) : base()
    {
        _cryptor = cryptor;
    }

    // <summary>
    // Encrypts the values of the properties of the entities that are being added or modified and implement IEncrypted
    // </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context is null)
            throw new ArgumentNullException(nameof(eventData), $"{nameof(eventData)} is null");

        var mutatingEntries = eventData.Context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in mutatingEntries)
        {
            if (entry.Entity is IEncrypted item)
            {
                var secretKey = _cryptor.GetQueryKey(entry.Entity.GetType().Name) ?? item.EncryptionKey;
                if (string.IsNullOrEmpty(secretKey))
                    continue;

                // Encrypt property if the value is typed as string, and the property is not null or empty
                foreach (var property in entry.Properties)
                {
                    var memberInfo = property.Metadata.PropertyInfo;
                    var encryptedAttribute = memberInfo?.GetCustomAttribute<EncryptedAttribute>();
                    if (encryptedAttribute == null)
                        continue;

                    var value = property.CurrentValue?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        property.CurrentValue = _cryptor.Encrypt(value, secretKey);
                }

                item.EncryptionKey = secretKey;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
