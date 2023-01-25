using System.Reflection;
using System.Runtime.Serialization;

namespace VirtualFinland.UserAPI.Helpers;

/// <summary>
///     Class to help deal with enums with funny names (starting with integer)
/// </summary>
public static class EnumUtilities
{
    /// <summary>
    ///     Tries to parse a string into an enum honoring EnumMemberAttribute if present
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static bool TryParseWithMemberName<T>(string value, out T result) where T : struct
    {
        result = default;

        if (string.IsNullOrEmpty(value))
            return false;

        var enumType = typeof(T);

        foreach (var name in Enum.GetNames(typeof(T)))
        {
            if (name.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = Enum.Parse<T>(name);
                return true;
            }

            var memberAttribute =
                enumType.GetField(name)?.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;

            if (memberAttribute is null)
                continue;

            if (memberAttribute.Value != null &&
                memberAttribute.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = Enum.Parse<T>(name);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Gets the enum value from a string honoring the EnumMemberAttribute if present
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetEnumValueOrDefault<T>(string value) where T : struct
    {
        return TryParseWithMemberName(value, out T result) ? result : default(T?);
    }

    /// <summary>
    ///     Tries to parse an enum value into an string honoring EnumMemberAttribute if present
    /// </summary>
    /// <param name="value">Enumeration value (0, 1, 2, ...)</param>
    /// <param name="result">Enum member name as string</param>
    /// <typeparam name="T">Enum</typeparam>
    /// <returns></returns>
    private static bool TryParseWithMemberValue<T>(T value, out string result) where T : struct
    {
        result = string.Empty;

        var name = value.ToString();

        if (string.IsNullOrEmpty(name)) return false;

        var memberAttribute =
            typeof(T).GetField(name)?.GetCustomAttribute(typeof(EnumMemberAttribute)) as EnumMemberAttribute;

        if (!string.IsNullOrEmpty(memberAttribute?.Value))
        {
            result = memberAttribute.Value;
            return true;
        }

        result = name;
        return true;
    }

    /// <summary>
    ///     Returns the string representation of enum member from a value honoring the EnumMemberAttribute if present
    /// </summary>
    /// <param name="value">Enumeration value (0, 1, 2, ...)</param>
    /// <typeparam name="T">Enum</typeparam>
    /// <returns></returns>
    public static string GetEnumMemberValueOrDefault<T>(T value) where T : struct
    {
        return TryParseWithMemberValue(value, out var result) ? result : string.Empty;
    }
}
