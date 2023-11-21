
namespace VirtualFinland.UserAPI.Helpers.Configurations;

/// <summary>
/// Enum-like class for mapping codesets resource names to codesets-endpoint paths
/// </summary>
public sealed class CodesetsResource
{
    private CodesetsResource(string value) { Value = value; }

    public string Value { get; private set; }

    public static CodesetsResource Countries => new("ISO3166CountriesURL");
    public static CodesetsResource Occupations => new("OccupationsEscoURL");
    public static CodesetsResource OccupationsFlat => new("OccupationsFlatURL");
    public static CodesetsResource Languages => new("ISO639Languages");

    public override string ToString()
    {
        return Value;
    }
}