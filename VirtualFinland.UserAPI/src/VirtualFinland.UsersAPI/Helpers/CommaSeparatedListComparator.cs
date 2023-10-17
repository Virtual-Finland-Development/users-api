using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace VirtualFinland.UserAPI.Helpers;

public class CommaSeparatedListComparator : ValueComparer<List<string>>
{
    public CommaSeparatedListComparator() : base(
        (c1, c2) => (c1 ?? new List<string>()).SequenceEqual(c2 ?? new List<string>()),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList()
    )
    { }
}
