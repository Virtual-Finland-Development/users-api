namespace VirtualFinland.UserAPI.Helpers;

public static class RegionMapper
{
    public static string FromIso_3166_2_ToCodeSet(string isoCode)
    {
        return isoCode switch
        {
            "FI-01" => "21",
            "FI-02" => "09",
            "FI-03" => "14",
            "FI-04" => "10",
            "FI-05" => "18",
            "FI-06" => "05",
            "FI-07" => "16",
            "FI-08" => "13",
            "FI-09" => "08",
            "FI-10" => "19",
            "FI-11" => "06",
            "FI-12" => "15",
            "FI-13" => "12",
            "FI-14" => "17",
            "FI-15" => "11",
            "FI-16" => "07",
            "FI-17" => "04",
            "FI-18" => "01",
            "FI-19" => "02",
            _ => throw new ArgumentOutOfRangeException(nameof(isoCode), $"{isoCode} is not valid ISO 3166-2:FI code")
        };
    }

    public static string FromCodeSetToIso_3166_2(string codeSetValue)
    {
        return codeSetValue switch
        {
            "21" => "FI-01",
            "09" => "FI-02",
            "14" => "FI-03",
            "10" => "FI-04",
            "18" => "FI-05",
            "05" => "FI-06",
            "16" => "FI-07",
            "13" => "FI-08",
            "08" => "FI-09",
            "19" => "FI-10",
            "06" => "FI-11",
            "15" => "FI-12",
            "12" => "FI-13",
            "17" => "FI-14",
            "11" => "FI-15",
            "07" => "FI-16",
            "04" => "FI-17",
            "01" => "FI-18",
            "02" => "FI-19",
            _ => throw new InvalidOperationException($"{codeSetValue} is not valid code set value")
        };
    }
}
