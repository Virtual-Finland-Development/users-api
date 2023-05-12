using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace VirtualFinland.UserAPI.Helpers;

public class CollectionValueComparer<T> : ValueComparer<ICollection<T>>
{
    public CollectionValueComparer() : base(
        (c1, c2) => Enumerable.SequenceEqual(c1 ?? Enumerable.Empty<T>(), c2 ?? Enumerable.Empty<T>()),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v == null ? 0 : v.GetHashCode())),
        c => c.ToHashSet())
    {
    }
}
