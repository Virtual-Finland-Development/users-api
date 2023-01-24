using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers;

/// <summary>
///     Interceptor used to automatically set created and modified property values on classes that inherit from
///     <see cref="Auditable" /> abstract class
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context is null)
            throw new ArgumentNullException(nameof(eventData), $"{nameof(eventData)} is null");

        var insertedEntries = eventData.Context.ChangeTracker
            .Entries()
            .Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity);

        foreach (var insertedEntry in insertedEntries)
            if (insertedEntry is Auditable auditableEntity)
                auditableEntity.Created = DateTime.UtcNow;

        var modifiedEntries = eventData.Context.ChangeTracker
            .Entries()
            .Where(x => x.State == EntityState.Modified)
            .Select(x => x.Entity);

        foreach (var modifiedEntry in modifiedEntries)
            if (modifiedEntry is Auditable auditableEntity)
                auditableEntity.Modified = DateTime.UtcNow;

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
