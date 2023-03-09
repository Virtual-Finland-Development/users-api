public class CodesetConfig
{
    public string? CodesetApiBaseUrl { get; set; }
    public string IsoCountriesUrl => CodesetApiBaseUrl + "/ISO3166CountriesURL";
    public string OccupationsEscoUrl => CodesetApiBaseUrl + "/OccupationsEscoURL";
    public string OccupationsFlatUrl => CodesetApiBaseUrl + "/OccupationsFlatURL";
    public string IsoLanguages => CodesetApiBaseUrl + "/ISO639Languages";
}