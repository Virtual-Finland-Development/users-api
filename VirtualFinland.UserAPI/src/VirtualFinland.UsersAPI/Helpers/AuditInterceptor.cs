using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
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
                case EntityState.Modified:
                case EntityState.Deleted:
                    if (entry.Entity is Auditable)
                    {
                        _logger.LogInformation($"AuditLog: {JsonConvert.SerializeObject(_CreateAuditLog(entry))}");
                    }
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private AuditLog _CreateAuditLog(EntityEntry entry)
    {
        var (primaryKeys, nonPrimaryKeys) = _GetLogMessageColumns(entry.Properties);
        return new AuditLog
        {
            TableName = entry.Metadata.DisplayName(),
            Action = entry.State.ToString(),
            KeyValues = primaryKeys,
            ChangedColumns = nonPrimaryKeys,
            EventDate = DateTime.UtcNow
        };
    }

    private Tuple<List<string>, List<string>> _GetLogMessageColumns(EntityEntry entry)
    {
        var primaryKeys = entry.Properties.Where(property => property.Metadata.IsPrimaryKey())
            .Select(property => $"{property.Metadata.Name} = {property.CurrentValue}");
        var nonPrimaryKeys = entry.Properties.Where(property => !property.Metadata.IsPrimaryKey() && entry.State != EntityState.Modified || property.IsModified)
            .Select(property => property.Metadata.Name);

        return new Tuple<List<string>, List<string>>(primaryKeys.ToList(), nonPrimaryKeys.ToList());
    }

    public record AuditLog
    {
        public string TableName { get; init; } = default!;
        public string Action { get; init; } = default!;
        public List<string> KeyValues { get; init; } = default!;
        public List<string> ChangedColumns { get; init; } = default!;
        public DateTime EventDate { get; init; } = default!;
    }
}
