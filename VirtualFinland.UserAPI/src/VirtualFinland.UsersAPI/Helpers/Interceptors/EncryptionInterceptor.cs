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
                if (mutatingEntries.Count() > 1)
                    throw new InvalidOperationException("Only one entity implementing IEncrypted can be added or modified at a time");

                var secretKey = _cryptor.ResolveQuery();

                // Encrypt property if the value is typed as string, and the property is not null or empty
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(string))
                    {
                        var value = property.CurrentValue?.ToString();
                        if (!string.IsNullOrEmpty(value))
                            property.CurrentValue = _cryptor.Encrypt(value, secretKey);
                    }
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
