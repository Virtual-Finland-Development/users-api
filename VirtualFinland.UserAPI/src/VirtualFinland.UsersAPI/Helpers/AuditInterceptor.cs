using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VirtualFinland.UserAPI.Models.UsersDatabase;

namespace VirtualFinland.UserAPI.Helpers;

public interface IAuditInterceptor : IInterceptor
{
}

/// <summary>
///     Interceptor used to automatically set created and modified property values on classes that inherit from
///     <see cref="Auditable" /> abstract class
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor, IAuditInterceptor
{
    private readonly ILogger<IAuditInterceptor> _logger;
    public AuditInterceptor(ILogger<IAuditInterceptor> logger) : base()
    {
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context is null)
            throw new ArgumentNullException(nameof(eventData), $"{nameof(eventData)} is null");

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is Auditable insertedEntity)
                    {
                        _logger.LogInformation(CreateAddedMessage(entry));
                        insertedEntity.Created = DateTime.UtcNow;
                    }
                    break;
                case EntityState.Modified:
                    if (entry.Entity is Auditable modifiedEntity)
                    {
                        _logger.LogInformation(CreateModifiedMessage(entry));
                        modifiedEntity.Modified = DateTime.UtcNow;
                    }
                    break;
                case EntityState.Deleted:
                    if (entry.Entity is Auditable)
                    {
                        _logger.LogInformation(CreateDeletedMessage(entry));
                    }
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    string CreateAddedMessage(EntityEntry entry)
        => $"Inserting {entry.Metadata.DisplayName()} "
        + CreateLogMessagePropertiesPart(entry.Properties);
    string CreateModifiedMessage(EntityEntry entry)
        => $"Updating {entry.Metadata.DisplayName()} "
        + CreateLogMessagePropertiesPart(entry.Properties.Where(
            property => property.IsModified || property.Metadata.IsPrimaryKey()
        ));
    string CreateDeletedMessage(EntityEntry entry)
        => $"Deleting {entry.Metadata.DisplayName()} "
        + CreateLogMessagePropertiesPart(entry.Properties.Where(
            property => property.Metadata.IsPrimaryKey()
        ));

    string CreateLogMessagePropertiesPart(IEnumerable<PropertyEntry> properties)
    {
        var primaryKeys = properties.Where(property => property.Metadata.IsPrimaryKey())
            .Select(property => $"{property.Metadata.Name} = {property.CurrentValue}");
        var nonPrimaryKeys = properties.Where(property => !property.Metadata.IsPrimaryKey())
            .Select(property => property.Metadata.Name);

        if (nonPrimaryKeys.Any())
            return $"{string.Join(", ", primaryKeys)} with {string.Join(", ", nonPrimaryKeys)}";
        return $"{string.Join(", ", primaryKeys)}";
    }
}
