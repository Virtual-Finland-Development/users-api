using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Repositories;

public class OccupationRoot
{
    public class Occupation
    {
        [JsonPropertyName("codeValue")]
        public string? Id { get; set; }
        
        [JsonPropertyName("prefLabel")]
        public LanguageTranslations? Name { get; set; }
        
        [JsonPropertyName("description")]
        public LanguageTranslations? Description { get; set; }
    }

    public class LanguageTranslations
    {
        [JsonPropertyName("fi")]
        public string? Finland { get; set; }
        [JsonPropertyName("sv")]
        public string? Swedish { get; set; }
        [JsonPropertyName("en")]
        public string? English { get; set; }
    }
    
    [JsonPropertyName("results")]

    public List<Occupation>? Occupations { get; set; }
}
