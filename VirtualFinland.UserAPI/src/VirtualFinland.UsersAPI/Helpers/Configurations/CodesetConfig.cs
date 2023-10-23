namespace VirtualFinland.UserAPI.Helpers.Configurations;

public class CodesetConfig
{
    public CodesetConfig(IConfiguration configuration)
    {
        CodesetApiBaseUrl = configuration.GetValue<string>("CodesetApiBaseUrl");
    }
    public CodesetConfig()
    {
    }

    public string? CodesetApiBaseUrl { get; set; }

    // @TODO: change structure so it can be type cheked (or find if there is a way to do it)
    public Dictionary<string, string> ResourceEndpoints { get; set; } = new Dictionary<string, string>() {
        { "Countries", "ISO3166CountriesURL" },
        { "Occupations", "OccupationsEscoURL" },
        { "OccupationsFlat", "OccupationsFlatURL" },
        { "Languages", "ISO639Languages" }
    };

    // @TODO: type safefy the resource name parameter
    public string GetResourceEndpoint(string resourceName)
    {
        if (CodesetApiBaseUrl is null)
        {
            throw new Exception("CodesetApiBaseUrl not defined");
        }
        if (!ResourceEndpoints.ContainsKey(resourceName))
        {
            throw new Exception($"Resource {resourceName} not defined");
        }
        return $"{CodesetApiBaseUrl}/{ResourceEndpoints[resourceName]}";
    }
}