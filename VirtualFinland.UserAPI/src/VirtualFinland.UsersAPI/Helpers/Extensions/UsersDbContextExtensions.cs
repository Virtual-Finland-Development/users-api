using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VirtualFinland.UserAPI.Data;

namespace VirtualFinland.UserAPI.Helpers.Extensions;

public static class UsersDbContextExtensions
{
    public static IEnumerable<IEntityType> GetDbSetEntityTypes(this UsersDbContext context)
    {
        var contextType = context.GetType();
        var dbSetProperties = contextType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var propertyInfo in dbSetProperties)
        {
            var entityType = context.Model.FindEntityType(propertyInfo.PropertyType.GetGenericArguments()[0]);
            if (entityType != null)
            {
                yield return entityType;
            }
        }
    }
}