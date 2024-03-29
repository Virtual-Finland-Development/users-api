﻿using System.Text.Json.Serialization;

namespace VirtualFinland.UserAPI.Models.Repositories;

public class OccupationFlatRoot
{
    public class Occupation
    {
        [JsonPropertyName("notation")]
        public string? Notation { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("prefLabel")]
        public LanguageTranslations? PrefLabel { get; set; }

        [JsonPropertyName("broader")]
        public List<string>? Broader { get; set; }
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

