using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace VirtualFinland.UserAPI.Helpers;

public class EnumCollectionJsonValueConverter<T> : ValueConverter<ICollection<T>, string> where T : struct, Enum
{
    public EnumCollectionJsonValueConverter() : base(
        v => JsonConvert.SerializeObject(v.Select(EnumUtilities.GetEnumMemberValueOrDefault).ToList()),
        v => (JsonConvert.DeserializeObject<ICollection<string>>(v) ?? new List<string>())
            .Select(e => (T)Enum.Parse(typeof(T), e))
            .ToList())
    {
    }
}
