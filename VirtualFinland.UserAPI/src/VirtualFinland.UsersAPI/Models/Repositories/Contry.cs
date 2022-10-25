using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Repositories;

public class Country
{
    public class CountryName
    {
        [JsonPropertyName("official")]
        public string? Official { get; set; }
        
        [JsonPropertyName("common")]
        public string? Common { get; set; }
    }
    
    [JsonPropertyName("name")]
    public CountryName? Name { get; set; }
    
    [JsonPropertyName("cca2")]
    public string? IsoCode { get; set; }
    
    [JsonPropertyName("cca3")]
    public string? IsoCodeTßhreeLetter { get; set; }
}