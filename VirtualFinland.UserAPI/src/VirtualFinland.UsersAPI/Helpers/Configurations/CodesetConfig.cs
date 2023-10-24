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

    public sealed class Resource
    {
        private Resource(string value) { Value = value; }

        public string Value { get; private set; }

        public static Resource Countries => new("ISO3166CountriesURL");
        public static Resource Occupations => new("OccupationsEscoURL");
        public static Resource OccupationsFlat => new("OccupationsFlatURL");
        public static Resource Languages => new("ISO639Languages");

        public override string ToString()
        {
            return Value;
        }
    }

    public string GetResourceEndpoint(Resource resource)
    {
        if (CodesetApiBaseUrl is null)
        {
            throw new Exception("CodesetApiBaseUrl not defined");
        }
        return $"{CodesetApiBaseUrl}/{resource}";
    }
}