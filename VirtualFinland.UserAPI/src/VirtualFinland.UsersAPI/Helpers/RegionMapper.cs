namespace VirtualFinland.UserAPI.Helpers;

public static class RegionMapper
{
    private static readonly Dictionary<string, string> Codes = new()
    {
        { "FI-01", "21" },
        { "FI-02", "09" },
        { "FI-03", "14" },
        { "FI-04", "10" },
        { "FI-05", "18" },
        { "FI-06", "05" },
        { "FI-07", "16" },
        { "FI-08", "13" },
        { "FI-09", "08" },
        { "FI-10", "19" },
        { "FI-11", "06" },
        { "FI-12", "15" },
        { "FI-13", "12" },
        { "FI-14", "17" },
        { "FI-15", "11" },
        { "FI-16", "07" },
        { "FI-17", "04" },
        { "FI-18", "01" },
        { "FI-19", "02" }
    };

    public static string FromIso_3166_2_ToCodeSet(string isoCode)
    {
        var result = Codes.SingleOrDefault(c => c.Key == isoCode);

        if (result.Value is null)
            throw new ArgumentOutOfRangeException(nameof(isoCode), $"{isoCode} is not valid ISO 3166-2:FI code");

        return result.Value;
    }

    public static string FromCodeSetToIso_3166_2(string codeSetValue)
    {
        var result = Codes.SingleOrDefault(c => c.Value == codeSetValue);

        if (result.Key is null)
            throw new ArgumentOutOfRangeException(nameof(codeSetValue), $"{codeSetValue} is not valid code set value");

        return result.Key;
    }
}
